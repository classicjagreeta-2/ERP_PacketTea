
var response = {};

function httpPOST(urlname, dataValue, IsJson) {

    var postData = "";
    if (IsJson) {
        postData = JSON.stringify(dataValue)
    }
    else {
        postData = dataValue;
    }
    var response = {};

    var message;
    $.ajax({
        type: "POST",
        url: urlname,
        data: postData,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        async: false,
        success: function (data) {

            response["status"] = "OK";
            response["data"] = data;

            // data;
        },
        error: function (e, x, settings, exception) {
            //var message;
            var statusErrorMap = {
                '400': "Server understood the request, but request content was invalid.",
                '401': "Unauthorized access.",
                '403': "Forbidden resource can't be accessed.",
                '500': "Internal server error.",
                '503': "Service unavailable."
            };

            if (e.status == 200) {
                return response["data"] = e.responseText;
            }
            if (x.status) {
                message = statusErrorMap[x.status];
                if (!message) {
                    message = "Unknown Error \n.";
                }
            } else if (exception == 'parsererror') {
                message = "Error.\nParsing JSON Request failed.";
            } else if (exception == 'timeout') {
                message = "Request Time out.";
            } else if (exception == 'abort') {
                message = "Request was aborted by the server";
            } else {
                message = "Unknown Error \n.";
            }
        }
    });

    return response;

}


function ddlLoadOnchangePOSTWithData(dData, dropdownid, parameterValue, firstValue) {



    var aRc = dData;


    $(dropdownid).empty();

    if (aRc.length > 0) {
        if (firstValue != null && firstValue != "")
            $(dropdownid).append("<option value=''>" + firstValue + "</option>");
    }


    // $('#ddl_copratename').children('option').remove();
    for (var i = 0; i <= aRc.length - 1; i++) {
        $(dropdownid).append("<option value='" + aRc[i].Value + "'>" + aRc[i].Text + "</option>");
    }



    //  $(dropdownid).selectpicker('refresh');


}


function ddlLoadOnchangePOST(MethodName, dropdownid, parameterValue, firstValue) {


    var x = httpPOST(MethodName,
        parameterValue, true);
    var aRc = x.data;


    $(dropdownid).empty();

    if (aRc.length > 0) {
        if (firstValue != null && firstValue != "")
            $(dropdownid).append("<option value=''>" + firstValue + "</option>");
    }


    // $('#ddl_copratename').children('option').remove();
    for (var i = 0; i <= aRc.length - 1; i++) {
        $(dropdownid).append("<option value='" + aRc[i].Value + "'>" + aRc[i].Text + "</option>");
    }



    //  $(dropdownid).selectpicker('refresh');


}

function ddlRest(dropdownid, firstValue) {
    $(dropdownid).empty();
    $(dropdownid).append("<option value=''>" + firstValue + "</option>");
}
function httpGet(urlname) {

    var message;
    $.ajax({
        type: "GET",
        url: urlname,
        contentType: "application/json; charset=utf-8",
        async: false,

        success: function (data) {

            response["status"] = "OK";
            response["data"] = data;

            // data;
        },
        error: function (e, x, settings, exception) {
            //var message;

            var statusErrorMap = {
                '400': "Server understood the request, but request content was invalid.",
                '401': "Unauthorized access.",
                '403': "Forbidden resource can't be accessed.",
                '500': "Internal server error.",
                '503': "Service unavailable."
            };
            if (x.status) {
                message = statusErrorMap[x.status];
                if (!message) {
                    message = "Unknown Error \n.";
                }
            } else if (exception == 'parsererror') {
                message = "Error.\nParsing JSON Request failed.";
            } else if (exception == 'timeout') {
                message = "Request Time out.";
            } else if (exception == 'abort') {
                message = "Request was aborted by the server";
            } else {
                message = "Unknown Error \n.";
            }
        }
    });

    return response;

}


function httpGetWithObj(urlname, dataValue) {


    var postData = JSON.stringify(dataValue)

    var response = {};
    var message;
    $.ajax({
        type: "GET",
        url: urlname,
        data: dataValue,
        //contentType: "application/json; charset=utf-8",
        //dataType: "json",
        async: false,
        success: function (data) {

            response["status"] = "OK";
            response["data"] = data;

            // data;
        },
        error: function (e, x, settings, exception) {
            //var message;

            var statusErrorMap = {
                '400': "Server understood the request, but request content was invalid.",
                '401': "Unauthorized access.",
                '403': "Forbidden resource can't be accessed.",
                '500': "Internal server error.",
                '503': "Service unavailable."
            };
            if (x.status) {
                message = statusErrorMap[x.status];
                if (!message) {
                    message = "Unknown Error \n.";
                }
            } else if (exception == 'parsererror') {
                message = "Error.\nParsing JSON Request failed.";
            } else if (exception == 'timeout') {
                message = "Request Time out.";
            } else if (exception == 'abort') {
                message = "Request was aborted by the server";
            } else {
                message = "Unknown Error \n.";
            }

            response["Error"] = message;
        }
    });

    return response;

}

function Onlynumbers(evt) {
    var theEvent = evt || window.event;
    var key = theEvent.keyCode || theEvent.which;
    key = String.fromCharCode(key);
    var regex = /[0-9]|\./;
    if (!regex.test(key)) {
        theEvent.returnValue = false;
        if (theEvent.preventDefault) theEvent.preventDefault();
    }
}

function ddlLoad(MethodName, dropdownid, firstValue) {

    var x = httpGet(MethodName);
    aRc = x.data;


    $(dropdownid).empty();

    if (aRc.length > 0) {
        $(dropdownid).append("<option value=''>" + firstValue + "</option>");
    }

    // $('#ddl_copratename').children('option').remove();
    for (var i = 0; i <= aRc.length - 1; i++) {
        $(dropdownid).append("<option value='" + aRc[i].Value + "'>" + aRc[i].Text + "</option>");
    }

    // $(dropdownid).selectpicker('refresh');


}

function ddlLoadParamter(MethodName, dropdownid, parameterValue, firstValue) {
    debugger;
    var x = httpPOST(MethodName,
        parameterValue, true);
    aRc = x.data;


    $(dropdownid).empty();

    if (aRc.length > 0) {
        $(dropdownid).append("<option value=''>" + firstValue + "</option>");
    }

    // $('#ddl_copratename').children('option').remove();
    for (var i = 0; i <= aRc.length - 1; i++) {
        $(dropdownid).append("<option value='" + aRc[i].Value + "'>" + aRc[i].Text + "</option>");
    }

    // $(dropdownid).selectpicker('refresh');


}


function ddlLoadOnchange(MethodName, dropdownid, parameterValue, firstValue) {

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


function msgSuccess(msgText) {
    $.confirm({
        title: 'Information...!',
        content: msgText,
        type: 'green',
        typeAnimated: true,
        icon: 'icon-smiley',
        buttons: {
            tryAgain: {
                text: 'Close',
                btnClass: 'btn-warning',
                action: function () {
                }
            },
            //close: function () {
            //}
        }
    });
}

function msgError(errormsg) {
    $.confirm({
        title: 'Encountered an error!',
        content: errormsg,
        type: 'red',
        typeAnimated: true,
        buttons: {
            tryAgain: {
                text: 'Close',
                btnClass: 'btn-red',
                action: function () {
                }
            },
            //close: function () {
            //}
        }
    });
}

function msgErrorMsg(errormsg) {
    $.confirm({
        title: 'Encountered an error!',
        content: errormsg,
        type: 'red',
        typeAnimated: true,
        buttons: {
            tryAgain: {
                text: 'Close',
                btnClass: 'btn-red',
                action: function () {
                }
            },
            //close: function () {
            //}
        }
    });
}
function getAge(dateString) {
    var now = new Date();
    var today = new Date(now.getYear(), now.getMonth(), now.getDate());

    var yearNow = now.getYear();
    var monthNow = now.getMonth();
    var dateNow = now.getDate();

    var dob = new Date(dateString.substring(6, 10),
        dateString.substring(0, 2) - 1,
        dateString.substring(3, 5)
    );

    var yearDob = dob.getYear();
    var monthDob = dob.getMonth();
    var dateDob = dob.getDate();
    var age = {};
    var ageString = "";
    var yearString = "";
    var monthString = "";
    var dayString = "";
    yearAge = yearNow - yearDob;

    if (monthNow >= monthDob)
        var monthAge = monthNow - monthDob;
    else {
        yearAge--;
        var monthAge = 12 + monthNow - monthDob;
    }

    if (dateNow >= dateDob)
        var dateAge = dateNow - dateDob;
    else {
        monthAge--;
        var dateAge = 31 + dateNow - dateDob;

        if (monthAge < 0) {
            monthAge = 11;
            yearAge--;
        }
    }

    age = {
        years: yearAge,
        months: monthAge,
        // days: dateAge
        days: 0
    };

    if (age.years > 1) yearString = " years";
    else yearString = " year";
    if (age.months > 1) monthString = " months";
    else monthString = " month";
    if (age.days > 1) dayString = " days";
    else dayString = " day";

    if ((age.years > 0) && (age.months > 0) && (age.days > 0))
        ageString = age.years + yearString + ", " + age.months + monthString + ", and " + age.days + dayString + "";
    else if ((age.years == 0) && (age.months == 0) && (age.days > 0))
        ageString = "Only " + age.days + dayString + " old!";
    else if ((age.years > 0) && (age.months == 0) && (age.days == 0))
        ageString = age.years + yearString;
    else if ((age.years > 0) && (age.months > 0) && (age.days == 0))
        ageString = age.years + yearString + " and " + age.months + monthString + "";
    else if ((age.years == 0) && (age.months > 0) && (age.days > 0))
        ageString = age.months + monthString + " and " + age.days + dayString + "";
    else if ((age.years > 0) && (age.months == 0) && (age.days > 0))
        ageString = age.years + yearString + " and " + age.days + dayString + "";
    else if ((age.years == 0) && (age.months > 0) && (age.days == 0))
        ageString = age.months + monthString + "";
    else ageString = "Oops! Could not calculate age!";

    return ageString;
}
function blockUIStart() {
    var div = '<div class="loader animation-start"><span class="circle delay-1 size-2"></span>';
    div += '<span class="circle delay-2 size-4"></span>';
    div += '<span class="circle delay-3 size-6"></span>';
    div += '<span class="circle delay-4 size-7"></span>';
    div += '<span class="circle delay-5 size-7"></span>';
    div += '<span class="circle delay-6 size-6"></span>';
    div += '<span class="circle delay-7 size-4"></span>';
    div += '<span class="circle delay-8 size-2"></span>';
    div += '</div>';
    $.blockUI({
        css: {
            backgroundColor: 'transparent',
            border: 'none'
        },
        message: div,
        baseZ: 1500,
        overlayCSS: {
            backgroundColor: '#FFFFFF',
            opacity: 0.7,
            cursor: 'wait'
        }
    });

}