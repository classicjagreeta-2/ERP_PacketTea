function configFilter($this, colArray) {
    setTimeout(function () {
        var tableName = $this[0].id;
        var columns = $this.api().columns();
        
        $.each(colArray, function (i, arg) {
            $('.dataTable th:eq(' + arg + ')').append('<img src="../t2/images/filter.png" class="filterIcon" onclick="showFilter(event,\'' + tableName + '_' + arg + '\')" />');
        });

        var template = '<div class="modalFilter">' +
            '<div class="modal-content">' +
            '{0}</div>' +
            '<div class="modal-footer">' +
            '<a href="#!" onclick="clearFilter(this, {1}, \'{2}\');"  class="btn btn-sm left waves-effect waves-light">Clear</a>' +
            '<a href="#!" onclick="performFilter(this, {1}, \'{2}\');"  class="btn btn-sm right waves-effect waves-light btn-filter">Ok</a>' +
            '<a href="#!" class="btn btn-sm right waves-effect waves-light btn-close-filter">Close</a>' +

            '</div>' +
            '</div>';
        $.each(colArray, function (index, value) {
            columns.every(function (i) {
                if (value === i) {
                    var column = this, content = '<input type="text" class="filterSearchText" onkeyup="filterValues(this)" /> <br/>';
                    var columnName = $(this.header()).text().replace(/\s+/g, "_");
                    var distinctArray = [];
                    column.data().each(function (d, j) {
                        if (distinctArray.indexOf(d) == -1) {
                            var id = tableName + "_" + columnName + "_" + j; // onchange="formatValues(this,' + value + ');
                            content += '<div><input type="checkbox" value="' + d + '"  id="' + id + '"/><label for="' + id + '"> ' + d + '</label></div>';
                            distinctArray.push(d);
                        }
                    });
                    var newTemplate = $(template.replace('{0}', content).replace('{1}', value).replace('{1}', value).replace('{2}', tableName).replace('{2}', tableName));
                    $('body').append(newTemplate);
                    modalFilterArray[tableName + "_" + value] = newTemplate;
                    content = '';
                }
            });
        });
    }, 50);
}
var modalFilterArray = {};
//User to show the filter modal
function showFilter(e, index) {
    $('.modalFilter').hide();
    $(modalFilterArray[index]).css({ left: 0, top: 0 });
    var th = $(e.target).parent();
    var pos = th.offset();
    //console.log(th);
    $(modalFilterArray[index]).width(th.width() * 0.75);
    $(modalFilterArray[index]).css({ 'left': pos.left, 'top': pos.top });
    $(modalFilterArray[index]).show();
    $('#mask').show();
    e.stopPropagation();
    $(".modalFilter .btn-close-filter").click(function () {
        $(this).parents(".modal-footer").parents(".modalFilter").hide();
    });

}

//This function is to use the searchbox to filter the checkbox
function filterValues(node) {
    var searchString = $(node).val().toUpperCase().trim();
    var rootNode = $(node).parent();
    if (searchString == '') {
        rootNode.find('div').show();
    } else {
        rootNode.find("div").hide();
        rootNode.find("div:contains('" + searchString + "')").show();
    }
}

//Execute the filter on the table for a given column
function performFilter(node, i, tableId) {
    var rootNode = $(node).parent().parent();
    var searchString = '', counter = 0;

    rootNode.find('input:checkbox').each(function (index, checkbox) {
        if (checkbox.checked) {
            searchString += (counter == 0) ? checkbox.value : '|' + checkbox.value;
            counter++;
        }

    });
	/*if(counter > 0){
		$(node).parent().parent().css("color","Red");
	}*/
    $('#' + tableId).DataTable().column(i).search(
        searchString,
        true, false
    ).draw();
    rootNode.hide();
    $('#mask').hide();
    
}

//Removes the filter from the table for a given column
function clearFilter(node, i, tableId) {
    var rootNode = $(node).parent().parent();
    rootNode.find(".filterSearchText").val('');
    rootNode.find('input:checkbox').each(function (index, checkbox) {
        checkbox.checked = false;
        $(checkbox).parent().show();
    });
    $('#' + tableId).DataTable().column(i).search(
        '',
        true, false
    ).draw();
    rootNode.hide();
    $('#mask').hide();
}