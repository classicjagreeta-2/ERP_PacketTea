/* =====================================================================
   Enable the three "TB Sales" menu items for:
     1. Production Entry  (ProductionEntryController -> Index)
     2. Market Return     (MarketReturnController    -> Index)
     3. Other Invoice     (OtherInvoiceController     -> Index)
   PROJECTNAME / MODULECODE = 'PT' (PacketTea)
   Oracle / SQL Developer syntax

   Unlike insert_vbmenu_done_tea_sample_draw_entry.sql /
   insert_vbmenu_done_blend_packing_entries.sql, the ORDERCODEs for these
   three items in VBMENU_PACKET_TEA (the PT module's MENU_TABLE) were NOT
   confirmed against the live database when this script was written -- no
   Oracle connection was available in that session. Run step 1 first, fill
   in the three ORDERCODE values it returns into step 2's DECLARE block,
   then proceed exactly like the two scripts above (which ARE already
   run/committed and confirmed working, and are the pattern this follows).
   ===================================================================== */

-- 1) Find the three menu tree entries and their ORDERCODEs. Adjust the LIKE
--    patterns if the actual menu names differ from what's shown in the app
--    (per the screenshots this was built from: "Production Entry",
--    "Market Return", "Other Invoice", all grouped under a "TB Sales" /
--    "Tea Bag Sales" node) -- match on whatever NAME actually comes back.
SELECT ID, NAME, ORDERCODE, TYPE, PID, PERDOTNETMENU
FROM   VBMENU_PACKET_TEA
WHERE  UPPER(NAME) LIKE '%TB SALES%'
   OR  UPPER(NAME) LIKE '%PRODUCTION%'
   OR  UPPER(NAME) LIKE '%MARKET RETURN%'
   OR  UPPER(NAME) LIKE '%OTHER INVOICE%'
ORDER  BY PID, ID;

-- 1b) Confirm none of the three already has a VBMENU_DONE mapping (replace
--     the three '<...>' placeholders with the ORDERCODEs step 1 returned
--     before running this).
-- SELECT ID, PROJECTNAME, MENUNAME, MENUCODE, CONTROLLER, ACTIONMETHOD, MODULECODE, ISACTIVE
-- FROM   CLASSICNEW.VBMENU_DONE
-- WHERE  MENUCODE IN ('<production-entry-ordercode>', '<market-return-ordercode>', '<other-invoice-ordercode>');
-- Expect 0 rows. If this returns rows, STOP -- use the CASE B / UPDATE pattern in
-- insert_vbmenu_done_tea_sample_draw_entry.sql instead of the INSERT below.

-- 2) Insert the three rows. IDs are derived from the current MAX(ID) so the
--    script stays correct even if more rows are added before it runs.
--    Fill in the three ORDERCODE constants below from step 1's result first.
DECLARE
    v_id NUMBER;

    v_menucode_prod    CONSTANT VARCHAR2(20) := '<production-entry-ordercode>';
    v_menucode_mretu   CONSTANT VARCHAR2(20) := '<market-return-ordercode>';
    v_menucode_otherinv CONSTANT VARCHAR2(20) := '<other-invoice-ordercode>';
BEGIN
    SELECT NVL(MAX(ID), 0) INTO v_id FROM CLASSICNEW.VBMENU_DONE;

    INSERT INTO CLASSICNEW.VBMENU_DONE
        (ID, PROJECTNAME, MENUNAME, MENUCODE, CONTROLLER, ACTIONMETHOD, MODULECODE, IS4SUBDOMAIN, ISACTIVE)
    VALUES
        (v_id + 1, 'PT', 'Production Entry', v_menucode_prod, 'ProductionEntry', 'Index', 'PT', 1, 1);

    INSERT INTO CLASSICNEW.VBMENU_DONE
        (ID, PROJECTNAME, MENUNAME, MENUCODE, CONTROLLER, ACTIONMETHOD, MODULECODE, IS4SUBDOMAIN, ISACTIVE)
    VALUES
        (v_id + 2, 'PT', 'Market Return', v_menucode_mretu, 'MarketReturn', 'Index', 'PT', 1, 1);

    INSERT INTO CLASSICNEW.VBMENU_DONE
        (ID, PROJECTNAME, MENUNAME, MENUCODE, CONTROLLER, ACTIONMETHOD, MODULECODE, IS4SUBDOMAIN, ISACTIVE)
    VALUES
        (v_id + 3, 'PT', 'Other Invoice', v_menucode_otherinv, 'OtherInvoice', 'Index', 'PT', 1, 1);
END;
/

-- 3) Review the inserted rows (fill in the same three ORDERCODEs here too).
-- SELECT ID, PROJECTNAME, MENUNAME, MENUCODE, CONTROLLER, ACTIONMETHOD, MODULECODE, ISACTIVE
-- FROM   CLASSICNEW.VBMENU_DONE
-- WHERE  MENUCODE IN ('<production-entry-ordercode>', '<market-return-ordercode>', '<other-invoice-ordercode>')
-- ORDER  BY ID;

-- 4) Only after confirming it looks right:
COMMIT;
-- ROLLBACK;
