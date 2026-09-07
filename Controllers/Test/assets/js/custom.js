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
    var $dateInputs = $("#ProductionDate, #InquiryDate, #OrderDueDate");

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
    $("#ProductionIcon").on("click", function () {
        $("#ProductionDate").datepicker("show");
    });

    $("#InquiryIcon").on("click", function () {
        $("#InquiryDate").datepicker("show");
    });

    $("#OrderDueDateIcon").on("click", function () {
        $("#OrderDueDate").datepicker("show");
    });


});


$(document).ready(function () {
    // Open specific popup
    $(".openPopup").click(function () {
        var popupId = $(this).data("popup");
        $(".popup-overlay, #" + popupId).fadeIn();
    });

    // Close only when "X" button is clicked
    $(".close-btn").click(function () {
        $(".popup-overlay, .popup-box").fadeOut();
    });
});
