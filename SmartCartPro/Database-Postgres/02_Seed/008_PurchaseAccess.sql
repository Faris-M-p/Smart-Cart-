-- =============================================================================
-- SmartCartPro | 02_Seed/008_PurchaseAccess.sql
-- Permissions and features for simplified Purchase Management
-- =============================================================================

INSERT INTO permissions (fk_module,permissioncode,permissionname,displayorder,isactive)
SELECT m.id_module, v.code, v.name, v.sort, TRUE
FROM (VALUES
    ('purchases.view', 'View purchases', 20),
    ('purchases.manage', 'Create and edit purchases', 21),
    ('purchases.approve', 'Approve purchases and add stock', 22),
    ('purchases.cancel', 'Cancel purchases', 23),
    ('purchasereturns.view', 'View purchase returns', 30),
    ('purchasereturns.manage', 'Create and edit purchase returns', 31),
    ('purchasereturns.approve', 'Approve purchase returns and reduce stock', 32),
    ('purchaseorders.view', 'View purchase orders', 35),
    ('purchaseorders.manage', 'Create and manage purchase orders', 36),
    ('supplierpayments.view', 'View supplier payments and statements', 40),
    ('supplierpayments.manage', 'Record and manage supplier payments', 41)
) AS v(code,name,sort)
JOIN modules m ON m.modulecode='PURCHASES' AND m.cancelled=FALSE
WHERE NOT EXISTS (SELECT 1 FROM permissions p WHERE p.permissioncode=v.code AND p.cancelled=FALSE);

-- Grant all purchase permissions to ADMIN role by default
INSERT INTO role_permissions (fk_role,fk_permission)
SELECT r.id_role, p.id_permission
FROM roles r CROSS JOIN permissions p
WHERE r.rolecode='ADMIN' AND r.cancelled=FALSE AND p.cancelled=FALSE
  AND (p.permissioncode LIKE 'purchases.%' OR p.permissioncode LIKE 'purchasereturns.%' 
       OR p.permissioncode LIKE 'purchaseorders.%' OR p.permissioncode LIKE 'supplierpayments.%')
  AND NOT EXISTS (SELECT 1 FROM role_permissions rp WHERE rp.fk_role=r.id_role AND rp.fk_permission=p.id_permission);

-- Module features
INSERT INTO module_features (fk_module,fk_parent_feature,featurecode,featurename,description,is_core,displayorder,isactive)
SELECT m.id_module, NULL, v.code, v.name, v.description, FALSE, v.sort, TRUE
FROM (VALUES
    ('PURCHASES.RETURNS', 'Purchase returns', 'Draft and approve goods returned to suppliers.', 50),
    ('PURCHASES.ORDERS', 'Purchase orders', 'Simple internal product requirement lists.', 55),
    ('PURCHASES.PAYMENTS', 'Supplier payments', 'Supplier transactions, payments and statements.', 60)
) AS v(code,name,description,sort)
JOIN modules m ON m.modulecode='PURCHASES' AND m.cancelled=FALSE
WHERE NOT EXISTS (SELECT 1 FROM module_features f WHERE f.featurecode=v.code AND f.cancelled=FALSE);

INSERT INTO installation_features (fk_feature,createdat)
SELECT f.id_feature, NOW()
FROM module_features f JOIN installation_modules im ON im.fk_module=f.fk_module
WHERE f.featurecode IN ('PURCHASES.RETURNS','PURCHASES.ORDERS','PURCHASES.PAYMENTS') AND f.cancelled=FALSE
  AND NOT EXISTS (SELECT 1 FROM installation_features x WHERE x.fk_feature=f.id_feature);

INSERT INTO module_preset_features (fk_preset,fk_feature)
SELECT p.id_preset, f.id_feature
FROM module_presets p CROSS JOIN module_features f
WHERE p.presetcode='PREMIUM' AND f.cancelled=FALSE
  AND f.featurecode IN ('PURCHASES.RETURNS','PURCHASES.ORDERS','PURCHASES.PAYMENTS')
  AND NOT EXISTS (SELECT 1 FROM module_preset_features x WHERE x.fk_preset=p.id_preset AND x.fk_feature=f.id_feature);
