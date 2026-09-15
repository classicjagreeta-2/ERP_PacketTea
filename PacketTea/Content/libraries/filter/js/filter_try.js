function configFilter($this, colArray) {
	setTimeout(function () {
		var tableName = $this[0].id;
		var columns = $this.api().columns();
        $.each(colArray, function (i, arg) {
           
            $('.dataTable th:eq(' + arg + ')').append('<img src="../t2/images/filter.png" class="filterIcon" onclick="showFilter(' + tableName + ',event,\'' + tableName + '_' + arg + '\')" />');
		});
	
	}, 50);
}

 
 
   
 

function filterDistinctRows(tblname) {
    var content = '<input type="text" class="filterSearchText" onkeyup="filterValues(this)" /> <br/>';

   
    
    var tableRows = $("#example tbody td:first-child");
    
 
    var storeValues = [];
    for (var i = 0; i < tableRows.length; ++i) { //for each item in the column
       // debugger;
        var text = tableRows[i].textContent;
        if (storeValues.indexOf(text) == -1) {
            storeValues.push(text);    
        }
       
    }
    
    var ctr = "";
    for (var j = 0; j <= storeValues.length -1 ; j++)
    {
        content += '<div><input name="sport" type="checkbox" value="' + storeValues[j] + '"  id="' + j + '"/><label for="' + j + '"> ' + storeValues[j] + '</label></div>';
    }

    var template = '<div class="modalFilter">' +
        '<div class="modal-content">' + content +
        '  </div>' +
        '<div class="modal-footer">' +
        '<a href="#!" onclick="clearFilter(example, {1}, \'{2}\');"  class="btn btn-sm left waves-effect waves-light">Clear</a>' +
        '<a href="#!" onclick="performFilter();"  class="btn btn-sm right waves-effect waves-light btn-filter">Ok</a>' +
        '<a href="#!" class="btn btn-sm right waves-effect waves-light btn-close-filter">Close</a>' +

        '</div>' +
        '</div>';

    $('body').append(template);
 
}

var modalFilterArray = {};
//User to show the filter modal
function showFilter(tblname,e, index) {
    debugger;
    
    filterDistinctRows(tblname);
	$('.modalFilter').show();
	 
	
}

 
 

 