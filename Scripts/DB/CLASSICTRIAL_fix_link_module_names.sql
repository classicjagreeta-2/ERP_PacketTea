-- CLASSICTRIAL: make USER_SCHEMA_LINK.MODULE_NAME match CLASSICNEW.MODULE.MODULE_NAME
-- The module-selection page only shows a module when the user's link carries the
-- module's exact name (every tile that shows matches; BP and WH didn't).
SET DEFINE OFF

-- BP: link said 'Workforce & Payroll Management'; module was renamed to '... - Bought Leaf' (2 rows)
UPDATE CLASSICNEW.USER_SCHEMA_LINK
   SET MODULE_NAME = 'Workforce & Payroll - Bought Leaf'
 WHERE MODULE_CODE = 'BP'
   AND NVL(MODULE_NAME, '-') <> 'Workforce & Payroll - Bought Leaf';

-- WH: link said 'WAREHOUSE'; module is 'Warehouse Management' (1 row)
UPDATE CLASSICNEW.USER_SCHEMA_LINK
   SET MODULE_NAME = 'Warehouse Management'
 WHERE MODULE_CODE = 'WH'
   AND NVL(MODULE_NAME, '-') <> 'Warehouse Management';

COMMIT;
