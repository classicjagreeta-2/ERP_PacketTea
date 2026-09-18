/* =====================================================================
   Fix the live VBMENU_DONE row for "AWR Entry" after renaming the
   controller AwrEntryController -> AWREntryController (Views/AwrEntry ->
   Views/AWREntry). Without this, the menu item points at a controller
   that no longer exists and the link breaks again.

   Tea Sample Draw Entry's VBMENU_DONE row (CONTROLLER='TeaSampleDrawEntry')
   is untouched by this rename -- that controller name was never renamed,
   only its internal "Awr"-named identifiers/routes -- so no DB change is
   needed for it.
   ===================================================================== */

-- 1) Confirm the current row before changing it.
SELECT ID, PROJECTNAME, MENUNAME, MENUCODE, CONTROLLER, ACTIONMETHOD, MODULECODE, ISACTIVE
FROM   CLASSICNEW.VBMENU_DONE
WHERE  MENUCODE = '01022010T';
-- Expect CONTROLLER = 'AwrEntry'.

-- 2) Update it to match the renamed controller.
UPDATE CLASSICNEW.VBMENU_DONE
SET    CONTROLLER = 'AWREntry'
WHERE  MENUCODE = '01022010T';

-- 3) Review the change.
SELECT ID, PROJECTNAME, MENUNAME, MENUCODE, CONTROLLER, ACTIONMETHOD, MODULECODE, ISACTIVE
FROM   CLASSICNEW.VBMENU_DONE
WHERE  MENUCODE = '01022010T';

-- 4) Only after confirming it looks right:
COMMIT;
-- ROLLBACK;
