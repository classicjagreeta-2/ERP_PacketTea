$(document).ready(function () {

    $(".privous").prop("disabled", true);
   
    // $(':checkbox').checkboxpicker();

    var counter = 0;

    var query = $('div.divpolicy'); ddlfilter

    // Hide all matching elements
    query.hide();


    $("#addrow").on("click", function () {
        var rowCount = $("#myTable tbody tr").length;
        var newRow = $("<tr data-id='" + rowCount+"'>");
        var cols = "";
        var rowcount = $("table.order-list >tbody >tr").length;
        // alert(rowcount);
        //var le = $("table.order-list tr").length;
        var columName = "";
        //if (le.length > 4) {
        // $("table.order-list tr").find("td:eq(4)").last().find("select option:selected").text();

        //}

        // var col1 = $("table.order-list tr:last td:eq(2)").find("select option:selected").text();
        // alert(col1);


        // <input type='checkbox' class='switch switch-small' />
        //id=ddl_relation_' + rowcount + ' 

        // id="ListFamilyDetails_0__NationId"
        // name="ListFamilyDetails[0].NationId"
        cols += '<td><input type="text" id=ListFamilyDetails_' + rowcount + '_NationId   class="form-control" name="ListFamilyDetails[' + rowcount + '].NationId" /></td>';
        cols += '<td><input type="text" id=ListFamilyDetails_' + rowcount + '_fullName   class="form-control" name="ListFamilyDetails[' + rowcount + '].fullName" /></td>';
        cols += '<td> <select id=ListFamilyDetails_' + rowcount + '_Sex  name="ListFamilyDetails[' + rowcount + '].Sex" class="form-control"><option value="Male">Male</option><option value="Female">Female</option><option value="Others">Others</option></select></td>';
        cols += '<td><input type="text" class="form-control" id=ListFamilyDetails_' + rowcount + '_DOB  name="ListFamilyDetails[' + rowcount + '].DOB" /></td>';
        // cols += '<td> <select   id=ListFamilyDetails_' + rowcount + '_Relation  name="ListFamilyDetails[' + rowcount + '].Relation"  class="form-control" onchange="ddl_relation_onchange(' + rowcount + ');"><option value="Spouse">Spouse</option><option value="Father">Father</option><option value="Mother">Mother</option> <option value="Daughter">Daughter</option> <option value="Son">Son</option></select></td>';
        cols += '<td> <select   style="z-index: 99999" id=ListFamilyDetails_' + rowcount + '_Relation  name="ListFamilyDetails[' + rowcount + '].Relation"   onchange="ddl_relation_onchange(' + rowcount + ');"></select></td>';
        cols += '<td><input type="text" class="form-control" id=ListFamilyDetails_' + rowcount + '_Height  name="ListFamilyDetails[' + rowcount + '].Height" /></td>';
        cols += '<td><input type="text" class="form-control" id=ListFamilyDetails_' + rowcount + '_Weight  name="ListFamilyDetails[' + rowcount + '].Weight" /></td>';
        cols += '<td><button type="button" class="ibtnDel btn btn-danger btn-icon"><i class="icon-remove3"></i></button></td>';

        //cols += '<td><input type="button" class="ibtnDel btn btn-md btn-danger btn-icon icon-grid" value="Delete"></td>';
        newRow.append(cols);
        $("table.order-list").append(newRow);
        counter++;


        //  var ctrlb = ' <td> <input class="styled hidden"  type="checkbox"><div class="btn-group" tabindex="0"><a class="btn btn-default">No</a><a class="btn active btn-success">Yes</a></div></td>';
        //var ctrl = $('<input/>').attr({ type: 'checkbox', name: 'chk' }).addClass("styled");
        // var ddlchk = '<td > <input type="checkbox"  /> </td>';
        columName = $("table.order-list tr").find("td:eq(4)").last().find("select option:selected").text();
        //  colVal = $("table.order-list tr").find($('input[name="hidden"]')).val();

        // $(':checkbox').checkboxpicker();
        //Relation Colum Bind
        adddropdown(rowcount);


        $("#tblbenefit tr:first").append("<th>" + columName  + "</th>");
        $("#tblbenefit tr:not(:first)").append('<td> <input name="ckCheck" id="columName_' + rowcount + '" type="checkbox" />   <input class="hid_' + rowcount+'" type="hidden" value="" /> </td > ');


        

        //var row = $('#tblimage');
        //var x=row.insertAfter
        var myform = $('#tblimage');
        var iter = 0;
        var colcount = myform.find('th').length;
        $('#tblimage thead tr').append('<th>' + columName + '</th>');
        //$('#tblimage tbody').append('<tr><td> <img id="img_photo_' + colcount + '" /></td><td><input type="button" id="btnCopy_' + iter + '" onclick="copy(' + colcount + ');" value="Save"/></td></tr>');
       // $('#tblimage tbody').append('<tr><td><input type="button" id="btnCopy_' + iter + '" onclick="copy(' + colcount + ');" value="Save"/></td></tr>');
       // $('#tblimage tbody').append('<td><input type="button" id="btnCopy_' + iter + '" onclick="copy(' + colcount + ');" value="Save"/></td></tr>');
        // alert(colcount);
        
        myform.find('tbody tr').each(function () {
            var trow = $(this);
            //if (trow.index() === 0) {
            //    trow.append('<th>' + columName + '</th>');
            //}
         //   alert(trow.index());
            if (trow.index() === 0) {
                trow.append('<td> <img id="img_photo_' + colcount + '" /></td>');
            }
            if (trow.index() === 1) {
                trow.append('<td><input type="button" id="btnCopy_' + iter + '" onclick="copy(' + colcount + ');" value="Save"/></td>');
            }

        });
        
        iter += 1;

    });



    $("table.order-list").on("click", ".ibtnDel", function (event) {
        $(this).closest("tr").remove();
        counter -= 1

        var rowId = $(this).closest('tr').attr("data-id");

        var ddpId = "ListFamilyDetails_" + rowId + "_Relation";
        var selectedRealtion = $("#" + ddpId + " option:selected").text();

        //alert(selectedRealtion);
         alert(rowId);

        //var colnum = $("#tblbenefit").closest("td:eq(1)").prevAll("td").length;
         var imageId = rowId;
         rowId = Number(rowId) + 2;
        
         //$('#tblbenefit').find('th:eq(' + rowId + ')').remove();
         //$('#tblbenefit').find('tr:eq(' + rowId + ')').remove();

         $('#tblbenefit tr').find('td:eq(' + rowId + '),th:eq(' + rowId + ')').remove();
         $('#tblimage tr').find('td:eq(' + imageId + '),th:eq(' + imageId + ')').remove();
       // $("tblbenefit td", event.delegateTarget).remove(":nth-child(" + rowId + ")");

        //var myIndex = rowId + 2 ;
        //$(this).parents("tblbenefit").find("tr").each(function () {

        //    $(this).find("td:eq(" + myIndex + "), th:eq(" + myIndex + ")").fadeOut('slow', function () {
        //        $(this).remove();                
        //    });
        //});


        //$("#tblbenefit tr").each(function () {
        //    $(this).find("td:eq(" + rowId + " )").remove();
        //    $(this).find("th:eq(" + rowId +")").remove();
        //});

        //$("#tblbenefit thead tr th").each(function () {
        //    var columnTitle = $.trim($(this).html());
            
        //    alert(columnTitle);
        //    if ($.trim($(this).html()) ==$.trim( selectedRealtion)) {
        //      //  $(this).remove();
        //    }
        //});

    });



    // Step show event
    $("#smartwizard").on("showStep", function (e, anchorObject, stepNumber, stepDirection, stepPosition) {
     //  alert("You are on step "+stepNumber+" now");
        if (stepPosition === 'first') {
            $("#prev-btn").addClass('disabled');
        } else if (stepPosition === 'final') {
            $("#next-btn").addClass('disabled');
        } else {
            $("#prev-btn").removeClass('disabled');
            $("#next-btn").removeClass('disabled');
        }
    });

    // Toolbar extra buttons
    //var btnFinish = $('<button></button>').text('Finish')
    //    .addClass('btn btn-info')
    //    .on('click', function () {
    //        alert('Finish Clicked');
    //    });
    //var btnCancel = $('<button></button>').text('Cancel')
    //    .addClass('btn btn-danger')
    //    .on('click', function () { $('#smartwizard').smartWizard("reset"); });


    var btnFinish = $('<button style="display:none" ></button>').text('Finish')
        .addClass('btn btn-info')
        .on('click', function () {
            alert('Finish Clicked');
        });
    var btnCancel = $('<button style="display:none"></button>').text('Cancel')
        .addClass('btn btn-danger')
        .on('click', function () { $('#smartwizard').smartWizard("reset"); });

    // Smart Wizard
    $('#smartwizard').smartWizard({
        selected: 0,
        theme: 'default',
        transitionEffect: 'fade',
        showStepURLhash: true,
        toolbarSettings: {
            toolbarPosition: 'both',
            toolbarExtraButtons: [btnFinish, btnCancel]
        }
    });


    // External Button Events
    $("#reset-btn").on("click", function () {
        // Reset wizard
        $('#smartwizard').smartWizard("reset");
        // location.reload();
        window.location.reload(false)
        return true;
    });

    $("#prev-btn").on("click", function () {
        // Navigate previous
        $('#smartwizard').smartWizard("prev");
        return true;
    });

    $("#next-btn").on("click", function () {
        // Navigate next
        
        $('#smartwizard').smartWizard("next");
        return true;
    });

    $("#theme_selector").on("change", function () {
        // Change theme
        $('#smartwizard').smartWizard("theme", $(this).val());
        return true;
    });

    // Set selected theme on page refresh
    $("#theme_selector").change();




});


function ddl_relation_onchange(id) {

    //var colName = $("#ddl_relation_" + id).val();
    var colName = $("#ListFamilyDetails_" + id + "_Relation option:selected").text();
    var colVal = $("#ListFamilyDetails_" + id + "_Relation option:selected").val();
    var rowcount = id + 2 ;

    var colbenefit = $('#tblbenefit').find('th').length;
    $('#tblbenefit').find('tr').find('th:eq(' + rowcount + ')').html(colName);
    $('#tblimage').find('tr').find('th:eq(' + id + ')').html(colName);
    $('.hid_' + id).val(colVal);
    //var myformB = $('#tblbenefit');    
    //var colcountB = myformB.find('th').length;
    //// alert(colcount);
    //myformB.find('th').each(function () {
    //   // alert(trow.index());
    //    var trow = $(this);
    //    if (trow.index() === id) {
    //        trow.html(colName);
    //    }


    //});

  
   
    //var myform = $('#tblbenefit');
    //var iter = 0;
    //var colcount = myform.find('th').length;
    //// alert(colcount);
    //myform.find('thead tr th').each(function () {
    //    var trow = $(this);
    //    alert(trow);
    //    if (trow.index() === id) {
    //        trow.html(colName);
    //    }


    //});

    //iter += 1;

    //var tbl2 = $('#tblbenefit');
    //var col = Number(id) + 2;
    //var iterr = 0;
    //var colcount = tbl2.find('th').length;
    //// alert(colcount);
    //tbl2.find('th').each(function () {
    //    var trow = $(this);
    //    if (trow.index() === col) {
    //        trow.html(colName)
    //    }
    //});
    //iterr += 1;

}

function ddlfilter() {
    // var Parameterval = $('#ddl_country').val();
    // alert('');
    //var obj = {};
    //var x = httpPOST("../Enrollment/ddload", obj, true);

    ddlLoad("GetProductName", "select[id=ddl_product]", "", "SELECT PRODUCT NAME");
}


function adddropdown(id) {
    //alert(id);
    ddlLoad("GetRelation", "select[id=ListFamilyDetails_" + id + "_Relation]", "", "SELECT RELATION");

}
function checkboxChange() {

    if ($("#chk_previous").prop("checked") == true) {
        $(".privous").prop("disabled", false);
    }
    else if ($("#chk_previous").prop("checked") == false) {
        $(".privous").prop("disabled", true);
    }
}

function ddlLoad(MethodName, dropdownid, parameterValue, firstValue) {

    //  GetProduct

    var aRc = "";
    if (MethodName == "GetCoperateCompanies" || MethodName == "GetInsuranceCompanies") {

        var DataStrigValue = {
            'strcountryid': parameterValue
        }
        var x = httpPOST("../Enrollment/" + MethodName,
            DataStrigValue, true);
        aRc = JSON.parse(x.data);

        //alert(x);
    }

    else if (MethodName == "GetProductName") {
        var DataStrigValue = {
            'strcountryid': $('#ddl_country').val(),
            'strinsuranceid': $('#ddl_insurancecompany').val()
        }
        var x = httpPOST("../Enrollment/" + MethodName,
            DataStrigValue, true);
        aRc = JSON.parse(x.data);
    }

    else {

        var x = httpPOST("../Enrollment/" + MethodName);
        aRc = JSON.parse(x.data);
    }


    $(dropdownid).empty();

    if (aRc.length > 0) {
        $(dropdownid).append("<option value=''>" + firstValue + "</option>");
    }

    // $('#ddl_copratename').children('option').remove();
    for (var i = 0; i <= aRc.length - 1; i++) {
        $(dropdownid).append("<option value='" + aRc[i].pkvalue + "'>" + aRc[i].dp + "</option>");
    }

    $(dropdownid).selectpicker('refresh');

    // $('#ddl_copratename').selectpicker('refresh');
    //for (var i = 0; i <= aRc.length - 1; i++) {
    //    $('#ddl_copratename').append(new Option(aRc[i].dp, aRc[i].pkvalue, false, false));
    //}



}



$('.policytype').click(function () {
    var val = $(this).attr("name");
    $('#txt_policytype').val(val)

    if (val == "CORPORATE") {
        // $('#ddl_copratename').prop('disabled', false);
        document.getElementById("ddl_copratename").disabled = false;
        document.getElementById("txt_empcode").disabled = false;
        
        $('#ddl_insurancecompany').empty();
        $('#ddl_insurancecompany').selectpicker('refresh');
        ddlLoad("GetCoperateCompanies", "select[id=ddl_copratename]", $('#ddl_country').val(), "SELECT CORPERATE NAME");
        var query = $('div.divpolicy');
        // Hide all matching elements
        query.hide();
    } else {
        // $('#ddl_copratename').prop('disabled', true);
        document.getElementById("ddl_copratename").disabled = true;
        document.getElementById("txt_empcode").disabled = true;
        ddlLoad("GetInsuranceCompanies", "select[id=ddl_insurancecompany]", $('#ddl_country').val(), "SELECT INSURANCE NAME");
        $('#ddl_copratename').empty();
        $('#ddl_copratename').selectpicker('refresh');
        var query = $('div.divpolicy');
        // Hide all matching elements
        query.show();
    }
});


$('.familytype').click(function () {
    var val = $(this).attr("name");
    $('#txt_familytype').val(val)
});

$(function () {
    $('#FileUpload1').change(function () {
        $('#Image1').hide();
        var reader = new FileReader();
        reader.onload = function (e) {
            $('#Image1').show();
            $('#Image1').attr("src", e.target.result);
            $('#Image2').show();
            $('#Image2').attr("src", e.target.result);
            $('#Image1').Jcrop({
                onChange: SetCoordinates,
                onSelect: SetCoordinates
            });
        }
        reader.readAsDataURL($(this)[0].files[0]);
    });

    $('#btnCrop').click(function () {
        var x1 = $('#imgX1').val();
        var y1 = $('#imgY1').val();
        var width = $('#imgWidth').val();
        var height = $('#imgHeight').val();
        var canvas = $("#canvas")[0];
        var context = canvas.getContext('2d');
        var img = new Image();
        img.onload = function () {
            canvas.height = height;
            canvas.width = width;
            context.drawImage(img, x1, y1, width, height, 0, 0, width, height);
            $('#imgCropped').val(canvas.toDataURL());
            $('[id*=btnUpload]').show();
        };
        img.src = $('#Image1').attr("src");
    });

});
function SetCoordinates(c) {
    $('#imgX1').val(c.x);
    $('#imgY1').val(c.y);
    $('#imgWidth').val(c.w);
    $('#imgHeight').val(c.h);
    $('#btnCrop').show();
};


function saveEnrollment(){
    var obj = {};   

    var x = httpPOST("../Enrollment/saveEnrollmentData",
        obj, true);

}
function tranferdata() {

    var obj = {};
    obj.tblbenefit = HTMLtbl.getData($('#tblbenefit'));
    obj.policyNumber = $('#txt_policyNumber').val();
 
    var x = httpPOST("../Enrollment/getBenefitDataTable",
        obj, true);

    aRc = x.data;
   // alert(aRc);
    if (aRc = "OK") {
       
        $("#btnSaveEnroll").click();
        $(".fileinput-upload").click();
       // saveEnrollment();

    }


}



var HTMLtbl = {
    getData: function (table) {
        var data = [];
        table.find('tr').not(':first').each(function (rowIndex, r) {
            var cols = [];
            $(this).find('td').each(function (colIndex, c) {

                var checkbox = $(this).find($('input[name="ckCheck"]'));

                if (checkbox.is(":checked")) {
                    //cols.push('"YES"');

                    var hidValue = $("[type='hidden']", this).val();
                    cols.push(hidValue);
                    // itArr.push('"' + $(this).text() + '"');
                } else {
                    cols.push( $(this).text());
                }

                //if ($(this).children(':text,:hidden,textarea,select').length > 0)
                //    cols.push($(this).children('input,textarea,select').val().trim());

                //// if dropdown text is needed then uncomment it and remove SELECT from above IF condition//
                //// else if ($(this).children('select').length > 0)
                //// cols.push($(this).find('option:selected').text());
                ////if (rowSelector.find("input[type='checkbox']").prop('checked'))
                //else if ($(this).children(':checkbox').length > 0)
                //{
                //    alert($(this).children(':checkbox').id);
                //    cols.push($(this).children(':checkbox').is(':checked') ? 1 : 0);
                //}


                //else
                //{
                //    cols.push($(this).text().trim());
                //}

            });
            data.push(cols);
        });
        return data;
    }
}

