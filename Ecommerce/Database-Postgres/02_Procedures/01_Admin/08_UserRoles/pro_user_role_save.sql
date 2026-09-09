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
    IN p_role_name TEXT DEFAULT NULL,
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
            FROM userroles
            WHERE LOWER(rolename) = LOWER(v_normalized_name)
              AND cancelled = FALSE
        ) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'Role name already exists.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;

        INSERT INTO userroles (
            rolename, description, issystemrole, isactive, createdat, cancelled
        )
        VALUES (
            v_normalized_name, p_description, FALSE, COALESCE(p_is_active, TRUE), v_now, FALSE
        )
        RETURNING id_userrole INTO v_id_user_role;

    ELSIF (p_user_action = 2) THEN
        IF (v_id_user_role <= 0) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'Invalid user role.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;

        IF NOT EXISTS (
            SELECT 1 FROM userroles
            WHERE id_userrole = v_id_user_role AND cancelled = FALSE
        ) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'User role not found.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;

        IF EXISTS (
            SELECT 1 FROM userroles
            WHERE id_userrole = v_id_user_role AND issystemrole = TRUE
        ) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'System roles cannot be modified.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;

        IF EXISTS (
            SELECT 1
            FROM userroles
            WHERE LOWER(rolename) = LOWER(v_normalized_name)
              AND cancelled = FALSE
              AND id_userrole <> v_id_user_role
        ) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'Role name already exists.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;

        UPDATE userroles
        SET rolename = v_normalized_name,
            description = p_description,
            isactive = COALESCE(p_is_active, TRUE),
            updatedat = v_now
        WHERE id_userrole = v_id_user_role;

        UPDATE userrolepermissions urp
        SET cancelled = FALSE,
            cancelledon = NULL,
            cancelledreason = NULL
        FROM tmp_perm_ids p
        WHERE p.id_permission = urp.fk_permission
          AND urp.fk_userrole = v_id_user_role;

        UPDATE userrolepermissions
        SET cancelled = TRUE,
            cancelledon = v_now,
            cancelledreason = 'Replaced during role update'
        WHERE fk_userrole = v_id_user_role
          AND cancelled = FALSE
          AND fk_permission NOT IN (SELECT id_permission FROM tmp_perm_ids);
    ELSE
        OPEN p_result FOR
            SELECT -1 AS response_code, 'Invalid user action.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    INSERT INTO userrolepermissions (
        fk_userrole, fk_permission, createdat, cancelled
    )
    SELECT
        v_id_user_role,
        p.id_permission,
        v_now,
        FALSE
    FROM tmp_perm_ids p
    WHERE NOT EXISTS (
        SELECT 1
        FROM userrolepermissions urp
        WHERE urp.fk_userrole = v_id_user_role
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
