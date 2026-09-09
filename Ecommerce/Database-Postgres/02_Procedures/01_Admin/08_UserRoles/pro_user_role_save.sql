/**********************************************************************
Created By  : Muhammed Faris
Created On  : 04/09/2026
Purpose     : Insert / Update User Role and replace permission mappings
              in one transaction.
  p_user_action = 1 Insert, 2 Update
  p_selected_permission_ids = JSON array of permission IDs, e.g. [14,15,16]
**********************************************************************/
CREATE OR REPLACE PROCEDURE pro_user_role_save(
    IN p_user_action INT,
    IN p_id_user_role INT DEFAULT 0,
    IN p_role_name TEXT,
    IN p_description TEXT DEFAULT NULL,
    IN p_is_active BOOLEAN DEFAULT TRUE,
    IN p_selected_permission_ids TEXT DEFAULT NULL,
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_now TIMESTAMP := NOW();
    v_normalized_name TEXT;
    v_permission_count INT := 0;
    v_id_user_role INT := COALESCE(p_id_user_role, 0);
BEGIN
    v_normalized_name := TRIM(COALESCE(p_role_name, ''));

    IF (v_normalized_name = '') THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'Role name is required.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    IF (
        p_selected_permission_ids IS NULL
        OR TRIM(p_selected_permission_ids) IN ('', '[]')
    ) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'At least one permission must be selected.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    CREATE TEMP TABLE IF NOT EXISTS tmp_perm_ids (
        id_permission INT NOT NULL PRIMARY KEY
    ) ON COMMIT DROP;

    DELETE FROM tmp_perm_ids;

    INSERT INTO tmp_perm_ids (id_permission)
    SELECT DISTINCT (value)::INT
    FROM jsonb_array_elements_text(p_selected_permission_ids::jsonb) AS t(value)
    WHERE value ~ '^\d+$'
      AND (value)::INT > 0;

    SELECT COUNT(*) INTO v_permission_count FROM tmp_perm_ids;

    IF (v_permission_count = 0) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'At least one permission must be selected.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    IF EXISTS (
        SELECT 1
        FROM tmp_perm_ids p
        WHERE NOT EXISTS (
            SELECT 1
            FROM permissions x
            WHERE x.id_permission = p.id_permission
              AND x.cancelled = FALSE
        )
    ) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'One or more selected permissions are invalid.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    IF (p_user_action = 1) THEN
        IF EXISTS (
            SELECT 1
            FROM user_roles
            WHERE LOWER(role_name) = LOWER(v_normalized_name)
              AND cancelled = FALSE
        ) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'Role name already exists.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;

        INSERT INTO user_roles (
            role_name, description, is_system_role, is_active, created_at, cancelled
        )
        VALUES (
            v_normalized_name, p_description, FALSE, COALESCE(p_is_active, TRUE), v_now, FALSE
        )
        RETURNING id_user_role INTO v_id_user_role;

    ELSIF (p_user_action = 2) THEN
        IF (v_id_user_role <= 0) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'Invalid user role.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;

        IF NOT EXISTS (
            SELECT 1 FROM user_roles
            WHERE id_user_role = v_id_user_role AND cancelled = FALSE
        ) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'User role not found.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;

        IF EXISTS (
            SELECT 1 FROM user_roles
            WHERE id_user_role = v_id_user_role AND is_system_role = TRUE
        ) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'System roles cannot be modified.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;

        IF EXISTS (
            SELECT 1
            FROM user_roles
            WHERE LOWER(role_name) = LOWER(v_normalized_name)
              AND cancelled = FALSE
              AND id_user_role <> v_id_user_role
        ) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'Role name already exists.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;

        UPDATE user_roles
        SET role_name = v_normalized_name,
            description = p_description,
            is_active = COALESCE(p_is_active, TRUE),
            updated_at = v_now
        WHERE id_user_role = v_id_user_role;

        UPDATE user_role_permissions urp
        SET cancelled = FALSE,
            cancelled_on = NULL,
            cancelled_reason = NULL
        FROM tmp_perm_ids p
        WHERE p.id_permission = urp.fk_permission
          AND urp.fk_user_role = v_id_user_role;

        UPDATE user_role_permissions
        SET cancelled = TRUE,
            cancelled_on = v_now,
            cancelled_reason = 'Replaced during role update'
        WHERE fk_user_role = v_id_user_role
          AND cancelled = FALSE
          AND fk_permission NOT IN (SELECT id_permission FROM tmp_perm_ids);
    ELSE
        OPEN p_result FOR
            SELECT -1 AS response_code, 'Invalid user action.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    INSERT INTO user_role_permissions (
        fk_user_role, fk_permission, created_at, cancelled
    )
    SELECT
        v_id_user_role,
        p.id_permission,
        v_now,
        FALSE
    FROM tmp_perm_ids p
    WHERE NOT EXISTS (
        SELECT 1
        FROM user_role_permissions urp
        WHERE urp.fk_user_role = v_id_user_role
          AND urp.fk_permission = p.id_permission
    );

    OPEN p_result FOR
        SELECT v_id_user_role AS response_code,
               CASE
                   WHEN p_user_action = 1 THEN 'User role created successfully.'
                   ELSE 'User role updated successfully.'
               END AS response_msg,
               TRUE AS status_code;

EXCEPTION
    WHEN OTHERS THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, SQLERRM AS response_msg, FALSE AS status_code;
END;
$$;
