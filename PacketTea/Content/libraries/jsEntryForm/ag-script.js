$(".exploder").click(function () {
  $(this).toggleClass("btn-primary btn-primary");

  $(this).children("i").toggleClass("fa-chevron-down fa-chevron-up");

  $(this).closest("tr").next("tr").toggleClass("hide");

  if ($(this).closest("tr").next("tr").hasClass("hide")) {
    $(this).closest("tr").next("tr").children("td").slideUp(350);
  } else {
    $(this).closest("tr").next("tr").children("td").slideDown(350);
  }
});

$(".new").click(function () {
  $(this).closest("tr").next("tr").toggleClass("hide2");

  if ($(this).closest("tr").next("tr").hasClass("hide2")) {
    $(this).closest("tr").next("tr").children("td").slideUp(350);
  } else {
    $(this).closest("tr").next("tr").children("td").slideDown(350);
  }
});

$(".newCancel").click(function () {
    $(this).closest("tr").toggleClass("hide2");

    if ($(this).closest("tr").hasClass("hide2")) {
      $(this).closest("tr").children("td").slideUp(350);
    } else {
      $(this).closest("tr").children("td").slideDown(350);
    }
});

$(".newDispatch").click(function () {
  $(this).closest("tr").next("tr").toggleClass("hide3");

  if ($(this).closest("tr").next("tr").hasClass("hide3")) {
    $(this).closest("tr").next("tr").children("td").slideUp(350);
  } else {
    $(this).closest("tr").next("tr").children("td").slideDown(350);
  }
});

$(".newDispatchCancel").click(function () {
    $(this).closest("tr").toggleClass("hide3");

    if ($(this).closest("tr").hasClass("hide3")) {
      $(this).closest("tr").children("td").slideUp(350);
    } else {
      $(this).closest("tr").children("td").slideDown(350);
    }
});

//   -------------------------------------------------------------------------------
var $TABLE = $('#dispatch-table');
var $BTN = $('#export-btn');
var $EXPORT = $('#export');

$('.table-add').click(function () {
  var $clone = $TABLE.find('tr.hide').clone(true).removeClass('hide table-line');
  $TABLE.find('table').append($clone);
});

$('.table-remove').click(function () {
  $(this).parents('tr').detach();
});

$('.table-up').click(function () {
  var $row = $(this).parents('tr');
  if ($row.index() === 1) return; // Don't go above the header
  $row.prev().before($row.get(0));
});

$('.table-down').click(function () {
  var $row = $(this).parents('tr');
  $row.next().after($row.get(0));
});

// A few jQuery helpers for exporting only
jQuery.fn.pop = [].pop;
jQuery.fn.shift = [].shift;

$BTN.click(function () {
  var $rows = $TABLE.find('tr:not(:hidden)');
  var headers = [];
  var data = [];
  
  // Get the headers (add special header logic here)
  $($rows.shift()).find('th:not(:empty)').each(function () {
    headers.push($(this).text().toLowerCase());
  });
  
  // Turn all existing rows into a loopable array
  $rows.each(function () {
    var $td = $(this).find('td');
    var h = {};
    
    // Use the headers from earlier to name our hash keys
    headers.forEach(function (header, i) {
      h[header] = $td.eq(i).text();   
    });
    
    data.push(h);
  });
  
  // Output the result
  $EXPORT.text(JSON.stringify(data));
});

// ------------------------------------------------------------------------------

function deleteRow(row)
    {
    //var i = row.parentNode.parentNode.rowIndex;
   // var row_index = row.parent('table').index(); 
        document.getElementById('POITable').deleteRow(i);
    } 
    
	
	 function insRow(row)
     {
         debugger;
       //  alert(row.closest('td').parent()[0].sectionRowIndex);
         var i = row.parentNode.parentNode.rowIndex;
         
         alert(i);
        var x = document.getElementById('POITable');
         var new_row = x.rows[1].cloneNode(true);
         var new_rowInSide = x.rows[2].cloneNode(true);
         debugger;
        var len = x.rows.length;
		 new_row.cells[1].getElementsByTagName('input')[0].value = "";
		 new_row.cells[4].getElementsByTagName('input')[0].value = "";
		 new_row.cells[5].getElementsByTagName('input')[0].value = "";
		 new_row.cells[6].getElementsByTagName('input')[0].value = "";
		 
		 new_row.cells[8].getElementsByTagName('input')[0].value = "";
		 new_row.cells[9].getElementsByTagName('input')[0].value = "";
        
         x.appendChild(new_row);
         x.appendChild(new_rowInSide);
    }
