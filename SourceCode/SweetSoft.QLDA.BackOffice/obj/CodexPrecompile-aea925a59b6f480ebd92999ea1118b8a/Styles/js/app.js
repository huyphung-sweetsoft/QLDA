
// ================ || HEADER MENU && FOOTER || ==================//
//======================================================//

function fixedMenu() {
    if ($(".wrapHeader").length) {
        var windowScrollTop = $(window).scrollTop();
        var heightHeader = 15;

        if ($(window).width() <= 575) heightHeader = 1;

        if (windowScrollTop > heightHeader) $("body").addClass("fixedMenu");
        else $("body").removeClass("fixedMenu");
    }
}
$(".wrapBtnCtrMenuHeader .btnCtrMenu").on("click", function (e) {
    $("body").toggleClass("showMenu wrapHidden");
    e.preventDefault();

    if ($(".showContact").length) {
        $("body").removeClass("showContact wrapHidden");
    }
});
$(".wrapperBTNclose ").on("click", function (e) {
    $("body").removeClass("showMenu wrapHidden");
    e.preventDefault();

    if ($(".showContact").length) {
        $("body").removeClass("showContact wrapHidden");
    }
});
$(".wrapperBtncloseMobile").on("click", function (e) {
    $("body").removeClass("showMenu wrapHidden");

    if ($(".showContact").length) {
        $("body").removeClass("showContact wrapHidden");
    }

    e.preventDefault();
});

$(".wrapCloseMenuHeaderMobile").on("click", function (e) {
    $("body").removeClass("showMenu wrapHidden");
    e.preventDefault();
});

$(document).on("mouseenter", ".itemMenuHeader.menuSub", function (e) {
    if ($(this).hasClass("activeMenu")) {
        $(this).find(".wrapMenuHeader").slideUp();

        $(this).removeClass("activeMenu");
    } else {
        var isChild = $(this).parents(".itemMenuHeader.menuSub").length > 0;
        if (isChild) {
            $(this).find(".wrapMenuHeader").slideUp();
            $(this).removeClass("activeMenu");
        }
        else {
            if ($(".itemMenuHeader.menuSub.activeMenu")) {
                $(".itemMenuHeader.menuSub").find(".wrapMenuHeader").slideUp();
                $(".itemMenuHeader.menuSub").removeClass("activeMenu");
            }
        }

        $(this).find(".wrapMenuHeader").toggle()

        $(this).toggleClass("activeMenu");
    }

    // e.preventDefault();
});

$(document).on("click", ".itemMenuHeader.menuSub", function (e) {
    e.stopPropagation();
});

$(document).on("click", ".Rootmenu .itemMenuHeader.menuSub", function () {
    if ($('.menuSetting').hasClass('showSub')) {
        $('.menuSetting .show-menu').toggle();
        $('.menuSetting').removeClass('showSub')
    }
});
$(document).on("mouseleave", ".itemMenuHeader.menuSub", function () {
    var $this = $(this);
    $this.find(".wrapMenuHeader").stop(true, true).slideUp();
    $this.removeClass("activeMenu");
});
$('body, html').on('click', function (e) {
    var target = $(e.target);
    if (
        e.type == "focusin" ||
        target.closest(this.element).length ||
        target.closest(this.container).length ||
        target.closest('.wrapMenuMainHeader').length
    ) return;

    if ($(".itemMenuHeader.menuSub.activeMenu")) {
        $(".itemMenuHeader.menuSub").find(".wrapMenuHeader").slideUp();
        $(".itemMenuHeader.menuSub").removeClass("activeMenu");
    }
});

//$('body, html').on('click', function (e) {
//    var target = $(e.target);
//    if (
//        e.type == "focusin" ||
//        target.closest(this.element).length ||
//        target.closest(this.container).length ||
//        target.closest('.wrapMenuMainHeader').length ||
//        target.closest('.menuSetting').length
//    ) return;

    //if ($('.menuSetting').hasClass('showSub')) {
    //    // $('.menuSetting .show-menu').slideUp();
    //    $('.menuSetting').removeClass('showSub')
    //}
//});


// ================ || KIỂM TRA NẾU SỐ LƯỢNG MENU QUÁ NHIỀU THÌ THÊM DROPDOWN || ==================//
//======================================================//
function responeMenuHeader() {
    if ($(window).width() > 1220) {
        var navbarNav = $('.headerTop .wrapperContentTop');

        var widthLanguage = $('.dropdown-language');
        var maxNavbarWidth = navbarNav.width() - $('.wrapLogoHeader').width() - $('.wrapperRight').width() - widthLanguage.width() - 90;

        console.log($('.wrapLogoHeader').width());

        var topLevelItemMenuHeaders = $('.listMenuHeader.Rootmenu > .itemMenuHeader')

        var totalWidth = 0;

        topLevelItemMenuHeaders.each(function () {
            totalWidth += $(this).outerWidth(true) + 15;

            if (totalWidth > maxNavbarWidth) {
                $(this).appendTo('.show-menu');
            }
        })
        if (totalWidth > maxNavbarWidth) {
            $('.btn-show-more').removeClass('d-none');
        } else {
            $('.btn-show-more').addClass('d-none');
        }
        $('.btn-show-more').on('click', function () {
            if ($(".itemMenuHeader.menuSub.activeMenu")) {
                $(".itemMenuHeader.menuSub").find(".wrapMenuHeader").slideUp();
                $(".itemMenuHeader.menuSub").removeClass("activeMenu");
            }

            if ($('.menuSetting').hasClass('showSub')) {
                $('.menuSetting .show-menu').addClass('d-none');
                $('.menuSetting').removeClass('showSub')

            } else {
                // $('.menuSetting .show-menu').slideDown();
                $('.menuSetting').addClass('showSub')
                $('.menuSetting .show-menu').removeClass('d-none');
                $('.menuSetting .show-menu').show();
            }
        });

        $('.btn-close').on('click', function () {
            $('.listMenuHeader.Rootmenu').find('.itemMenuHeader').each(function () {
                if ($(this).offset().left > maxNavbarWidth) {
                    $(this).addClass('d-none');
                }
            });
            $('.btn-show-more').removeClass('d-none');
            $(this).addClass('d-none');
        });
    }
}


$(document).ready(function () {
  function adjustMainContentHeight() {
    if ($('.main-content') && $(window).width() > 560) {
      var headerHeight = $('.wrapHeader').outerHeight();
      var footerHeight = $('.footer').outerHeight();
      var totalHeight = headerHeight + footerHeight;
        var newHeight = 'calc(100vh - ' + totalHeight + 'px - 20px)';
        $('.min-h-sreen').css('min-height', newHeight);
        if ($('.js-calc-scroll-content').length) {
            const diffHeight = parseInt($('.js-calc-scroll-content').attr("data-fixed-height") || 0);
            newHeight = 'calc(100vh - ' + (totalHeight + 180) + diffHeight + 'px)';
            $('.js-calc-scroll-content').css('height', newHeight)
        }
        if ($(".js-calc-scroll-content-no-padding").length) {
            newHeight = 'calc(100vh - ' + (totalHeight + 140) + 'px)';
            $('.js-calc-scroll-content-no-padding').css('height', newHeight)
        }
        if ($(".js-calc-scroll-sku").length) {
            newHeight = 'calc(100vh - ' + (totalHeight + 344) + 'px)';
            $('.js-calc-scroll-sku').css('height', newHeight)
        }
    }
  }
  $(window).on('load resize', adjustMainContentHeight);
  function adjustCardHeight() {
    if ($('.card-h')) {
      var headerHeight = $('.wrapHeader').outerHeight();
      var footerHeight = $('.footer').outerHeight();
      var totalHeight = headerHeight + footerHeight;
      var newHeight = 'calc(100vh - ' + totalHeight + 'px - 80px)';
      $('.card-h').css('min-height', newHeight);
    }
  }
  $(window).on('load resize', adjustCardHeight);
});

// ================ || END HEADER MENU && FOOTER || ==================//
//======================================================//


// ===================== || fIXED COLUMN TABLE || ===================//
//=====================================================================//
var prevColumnsWidth = {}; // Mảng để lưu trữ tổng chiều rộng của các cột đã được chọn trước đó
$(document).on("click", ".toggle-column", function () {

    var column = $(this).data("column");
    if ($(this).hasClass("active")) {
        $(".datatable td:nth-child(" + column + "), .datatable th:nth-child(" + column + ")").addClass("fixed-column");
        updatePrevColumnsWidth(column); // Cập nhật lại prevColumnsWidth cho cột mới được chọn
        updateWidth(column); // Cập nhật lại chiều rộng của các cột đã được chọn trước đó
        $(this).removeClass("active");
    } else {
        $(".datatable td:nth-child(" + column + "), .datatable th:nth-child(" + column + ")").removeClass("fixed-column");
        delete prevColumnsWidth[column]; // Xóa tổng chiều rộng của cột hiện tại từ mảng
        $(".datatable td:nth-child(" + column + "), .datatable th:nth-child(" + column + ")").css({
            "position": "",
            "background-color": "",
            "z-index": "",
            "left": ""
        });
        updateWidth(column); // Cập nhật lại chiều rộng của các cột đã được chọn trước đó
        $(this).addClass("active");
    }
})

// Hàm cập nhật lại prevColumnsWidth cho cột mới được chọn
function updatePrevColumnsWidth(column) {
    var totalWidth = 0;
    for (var key in prevColumnsWidth) {
        totalWidth += prevColumnsWidth[key];
    }
    prevColumnsWidth[column] = $(".datatable td.fixed-column:nth-child(" + column + ")").outerWidth();
    totalWidth += prevColumnsWidth[column];
    // Cập nhật lại prevColumnsWidth cho các cột đã được chọn trước đó
    for (var key in prevColumnsWidth) {
        if (parseInt(key) < column) {
            prevColumnsWidth[key] = prevColumnsWidth[key] / totalWidth * (totalWidth);
        }
    }
}

// Hàm cập nhật lại chiều rộng của các cột đã được chọn trước đó
function updateWidth(column) {
    var totalWidth = 0;
    for (var key in prevColumnsWidth) {
        totalWidth += prevColumnsWidth[key];
    }
    var left = 0;
    for (var key in prevColumnsWidth) {
        $(".datatable td:nth-child(" + key + "), .datatable th:nth-child(" + key + ")").css("left", left + "px");
        left += prevColumnsWidth[key];
    }
}

$('tbody').on('click', 'tr', function () {
    if ($(this).hasClass("unfocused") || $(this).hasClass("focused")) {
        $(this).removeClass("unfocused");
        $(this).removeClass("focused");
    }
    if (($("table").hasClass("focus-on"))) {
        $(this).toggleClass('selected');
    }
});


$(document).ready(function () {
    // ================ || MENU TAB VERTICAL || ==================//
    //======================================================//
    let menu = document.querySelector('.menu');
    let toggle = document.querySelector('.toggle');

    if (toggle && menu) {
        toggle.addEventListener('click', () => {
            menu.classList.toggle('active');
        });
    }

    // ========|| HIỂN THỊ NÚT SORT TABLE || ===========
    $(function () {
        $('table').on('click', '.thTable', function () {
            var th = $(this).closest('th');
            var thClass = th.hasClass('asc') ? 'desc' : 'asc';
            th.siblings().removeClass('asc desc');
            th.removeClass('asc desc').addClass(thClass);
        });
    });
});

// ================ || Chỉnh full đội cao khung nếu nội dung không nhiều || ==================//
//======================================================//
$(window).on("load", function () {
    responeMenuHeader();
    fixedMenu();
});

$(window).on("scroll", function () {
    fixedMenu();
});

$(window).on("resize", function () {
   // responeMenuHeader();
})
$(document).ready(function () {
    // ==========|| sử dụng cho khi xuống mobile sẽ chuyển sang dropdown ||==========================//
    if ($('.action-button-reponesive')) {
        var buttonsReponesive = $('.list-btn-action-reponesive').find('.btn');
        $(window).on('resize', () => {
            if (buttonsReponesive.length > 1 && $(window).width() < 992) {
                $('.dropdown-toggle-reponesive').show();
                $('.list-btn-action-reponesive').addClass('dropdown-menu list-btn-action-reponesive-temp');
                $('.list-btn-action-reponesive').removeClass('list-btn-action-reponesive')
            } else {
                $('.list-btn-action-reponesive-temp').removeClass('dropdown-menu');
                $('.list-btn-action-reponesive-temp').addClass('list-btn-action-reponesive')
                $('.dropdown-toggle-reponesive').hide();
            }
        })
    }
});

$(document).ready(function () {
    if ($('.dropdown-toggle-action')) {
        $('.dropdown-toggle-action').click(function () {
            var $tr = $(this).closest('tr');
            $('.dt-responsive tr').css('z-index', '');
            $tr.css('z-index', 500);
        });
    }
});