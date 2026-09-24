/* =====================================================================
   Point two PacketTea "Enquiries and reports" menu items at the ported
   report screens (PT module, VBMENU_PACKET_TEA ID = 'REPRT'):

     "Blend Sheet Printing"  09160000F  (VBMENU_DONE ID 362)
         -> FullsStockActual / Index   "Stock Report - (Batch wise)"
     "Purchase Tea Stock"    09170000F  (VBMENU_DONE ID 364)
         -> StockReport / Index        "Stock Report"

   Oracle / SQL Developer syntax

   Verified against CLASSICNEW on 2026-09-23: both rows already exist in
   VBMENU_DONE (MODULECODE = 'PT') but still point at the old VB6 report
   forms (Con_HO_04011700F / Con_HO_04011800F), which open
   /ReportsRedirect/OpenPage -- a 404 in this app. So this is an UPDATE.
   The menu builds each link as /<CONTROLLER>/<ACTIONMETHOD>?MId=REPRT;
   Global.asax.cs lets those two controllers through the REPRT redirect.
   ===================================================================== */

-- 1) Current mappings (expect 2 rows: IDs 362 and 364).
SELECT ID, MENUNAME, MENUCODE, CONTROLLER, ACTIONMETHOD, MODULECODE, IS4SUBDOMAIN, ISACTIVE
FROM   CLASSICNEW.VBMENU_DONE
WHERE  MENUCODE IN ('09160000F', '09170000F') AND MODULECODE = 'PT';

-- 2) Re-point them.
UPDATE CLASSICNEW.VBMENU_DONE
   SET MENUNAME     = 'Stock Report - (Batch wise)',
       CONTROLLER   = 'FullsStockActual',
       ACTIONMETHOD = 'Index',
       ISACTIVE     = 1
 WHERE MENUCODE = '09160000F' AND MODULECODE = 'PT';

UPDATE CLASSICNEW.VBMENU_DONE
   SET MENUNAME     = 'Stock Report',
       CONTROLLER   = 'StockReport',
       ACTIONMETHOD = 'Index',
       ISACTIVE     = 1
 WHERE MENUCODE = '09170000F' AND MODULECODE = 'PT';

-- 3) Review, then:
SELECT ID, MENUNAME, MENUCODE, CONTROLLER, ACTIONMETHOD, MODULECODE, ISACTIVE
FROM   CLASSICNEW.VBMENU_DONE
WHERE  MENUCODE IN ('09160000F', '09170000F') AND MODULECODE = 'PT';

COMMIT;
-- ROLLBACK;
