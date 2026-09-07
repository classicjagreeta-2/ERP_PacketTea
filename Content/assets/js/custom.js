$(document).ready(function () {
    // Accordion toggle
    $('.accordion-header').click(function () {
        // $('.accordion-header').not(this).removeClass('active').next('.accordion-body').slideUp();
        $(this).toggleClass('active').next('.accordion-body').slideToggle();
    });
    $('.accordion-header').first().addClass('active').next('.accordion-body').show();


});


$(document).ready(function () {
    $('.toggle-icon').on('click', function () {
        const $icon = $(this);
        const $mainRow = $icon.closest('tr');
        const $subTableRow = $mainRow.next('.sub-table-row');

        // Toggle visibility
        $subTableRow.toggle();

        // Toggle icon class
        if ($icon.hasClass('fa-plus')) {
            $icon.removeClass('fa-plus').addClass('fa-minus');
        } else {
            $icon.removeClass('fa-minus').addClass('fa-plus');
        }
    });
});

$(function () {
    // Correct selector
    var $dateInputs = $("#VoucherDate, #ValueDate, #OrderDueDate, #BillDate, #VoucherDate2");

    // If any were pre-initialized or had class stubbed in markup, clean them
    $dateInputs.each(function () {
        var $el = $(this);
        if ($el.data("datepicker")) $el.datepicker("destroy");
        $el.removeClass("hasDatepicker");
    });

    // One overlay in the page
    function ensureOverlay() {
        if (!$(".ui-datepicker-overlay").length) {
            $("body").append('<div class="ui-datepicker-overlay"></div>');
        }
    }

    var activeInput = null;

    $dateInputs.datepicker({
        dateFormat: "dd-mm-yy",
        changeMonth: true,   // enable month dropdown
        changeYear: true,    // enable year dropdown
        yearRange: "2000:2035", // customize year range
        showAnim: "", // snappy; animations can fight stacking contexts
        beforeShow: function (input) {
            activeInput = input;

            ensureOverlay();
            $(".ui-datepicker-overlay").show();

            $("body").addClass("datepicker-open");

            setTimeout(function () {
                $("#ui-datepicker-div")
                    .addClass("force-block")
                    .css({ zIndex: 99999 });
            }, 0);
        },
        onChangeMonthYear: function () {
            setTimeout(function () {
                $("#ui-datepicker-div")
                    .addClass("force-block")
                    .css({ zIndex: 99999 });
            }, 0);
        },
        onClose: function () {
            $(".ui-datepicker-overlay").hide();
            $("#ui-datepicker-div").removeClass("force-block");
            $("body").removeClass("datepicker-open");
            activeInput = null;
        }
    });

    // Close when clicking overlay
    $(document).on("click", ".ui-datepicker-overlay", function () {
        if (activeInput) $(activeInput).datepicker("hide");
    });

    // Calendar icon click
    $("#VoucherIcon").on("click", function () {
        $("#VoucherDate").datepicker("show");
    });

    $("#VoucherIcon2").on("click", function () {
        $("#VoucherDate2").datepicker("show");
    });

    $("#ValueIcon").on("click", function () {
        $("#ValueDate").datepicker("show");
    });

    $("#OrderDueDateIcon").on("click", function () {
        $("#OrderDueDate").datepicker("show");
    });

    $("#BillDateIcon").on("click", function () {
        $("#BillDate").datepicker("show");
    });


});

// popup

$(document).ready(function () {

    // OPEN POPUP
    $(".openPopup").on("click", function (e) {
        e.preventDefault();
        debugger
        const popupId = $(this).data("popup");
        const $popup = $("#" + popupId);

        // if child popup
        if ($popup.hasClass("child")) {
            $(".child-overlay").fadeIn(200);
            $popup.fadeIn(200);
        } else {
            $(".popup-overlay").fadeIn(200);
            $popup.fadeIn(200);
        }
    });

    // CLOSE BUTTON HANDLER
    $(".popup-overlay, .child-overlay").on("click", ".close-btn", function () {

        const $popup = $(this).closest(".popup-box");

        // closing child popup
        if ($popup.hasClass("child")) {
            $popup.fadeOut(200);
            $(".child-overlay").fadeOut(200);
        }
        // closing parent popup
        else {
            // ❌ BLOCK parent close if child is open
            if ($(".popup-box.child:visible").length > 0) return;

            $popup.fadeOut(200, function () {
                $(".popup-overlay").fadeOut(200);
            });
        }
    });

});





// search

document.addEventListener('DOMContentLoaded', () => {

    const input = document.getElementById('searchInput');
    const searchBtn = document.querySelector('.searchbtn');
    const clearBtn = document.querySelector('.clearbtn');

    input.addEventListener('input', () => {
        if (input.value.trim() !== '') {
            searchBtn.style.display = 'none';
            clearBtn.style.display = 'block';
        } else {
            searchBtn.style.display = 'block';
            clearBtn.style.display = 'none';
        }
    });

    clearBtn.addEventListener('click', () => {
        input.value = '';
        input.focus();
        searchBtn.style.display = 'block';
        clearBtn.style.display = 'none';
    });

});


// custom select

$(document).ready(function () {

    // open dropdown
    $('.select-input').on('click', function (e) {
        e.stopPropagation();

        let $customSelect = $(this).closest('.custom-select');

        //$('.dropdown').not($customSelect.find('.dropdown')).hide();
        $('.dropdown').not('.dropdown1').not($customSelect.find('.dropdown')).hide();
        $customSelect.find('.dropdown').toggle();
    });

    // prevent close on search
    $('.select-box').on('click', function (e) {
        e.stopPropagation();
    });

    // search filter
    $('.select-box').on('keyup', function () {
        let value = $(this).val().toLowerCase();

        $(this).closest('.dropdown').find('.options li').each(function () {
            $(this).toggle($(this).text().toLowerCase().includes(value));
        });
    });

    // select item → bind placeholder + title
    $('.options li').on('click', function (e) {
        e.stopPropagation();

        let selectedText = $(this).text();
        let $customSelect = $(this).closest('.custom-select');
        let $input = $customSelect.find('.select-input');

        // bind value
        $input
            .val(selectedText)                 // optional (if you want visible text)
            .attr('placeholder', selectedText) // placeholder text
            .attr('title', selectedText);      // tooltip

        $customSelect.find('.dropdown').hide();
    });

    // close outside
    $(document).on('click', function () {
        //$('.dropdown').hide();
        $('.dropdown').not('.dropdown1').hide();
    });

});



$(document).ready(function () {

    $(document).on('focus click', '.inputpicker-input', function () {

        var $input = $(this);
        var $group = $input.closest('.tds-group');

        setTimeout(function () {

            // current visible dropdown only
            var $dropdown = $('.inputpicker-wrapped-list:visible');

            if ($dropdown.length) {

                $dropdown.appendTo($group);

                // $dropdown.css({
                //     position: 'absolute',
                //     top: $input.outerHeight() + 5,
                //     left: 0,
                //     width: '100%',
                //     zIndex: 9999
                // });

            }

        }, 100);

    });

});


