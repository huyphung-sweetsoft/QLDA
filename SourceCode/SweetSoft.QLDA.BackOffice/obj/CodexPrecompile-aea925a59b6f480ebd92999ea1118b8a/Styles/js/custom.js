// find content top
function findContentTop() {
    if(!$('.wrapTitleAndBtnCtrl .wrapBtnRight ').length) $('body').addClass('noBtnCtrRight');
}
// end find content top

// button ctrl list
$('.numberFourBtn .btnShowBtn').on('click', function() {
    $('.numberFourBtn').toggleClass('show');
});

$('body, html').on('click', function(e){
    var target = $(e.target);
    if(
        e.type == "focusin" ||
        target.closest(this.element).length ||
        target.closest(this.container).length ||
        target.closest('.numberFourBtn').length
    ) return;

    if($('.numberFourBtn').hasClass('show') && $('.numberFourBtn').length){
        $('.numberFourBtn').removeClass('show');
    }
});

function fixedButtonCtrl() {
    if($('.wrapTitleAndBtnCtrl').length) {
        var windowScrollTop = $(window).scrollTop();

        var heightScroll = $('.wrapTitleAndBtnCtrl .titleFormMain').height() + $('.wrapTitleAndBtnCtrl .page-title-box').height() + 45;

        var heightItem = $('.wrapTitleAndBtnCtrl .listBtnRight').height();

        if($(window).width() <= 1199) {
            heightItem = 31
        }

        $('.wrapTitleAndBtnCtrl .wrapBtnRight').attr('style', 'height: '+ heightItem +'px');

        if(windowScrollTop > heightScroll) $('.wrapTitleAndBtnCtrl').addClass('fixed');
        else $('.wrapTitleAndBtnCtrl').removeClass('fixed');
    }
}
// end button ctrl list

// set content col
function setContentCol() {    
    var colItem = $('.rowSplit .colItem');

    $('.rowSplit').parents('.offcanvas').addClass('fullCanvas');
    
    if(colItem.length) {
        colItem.each(function() {
            if($(this)[0].children.length ==0) {
                $(this).addClass('d-none');

                $('.rowSplit').parents('.offcanvas').removeClass('fullCanvas');
            } else {
                $(this).removeClass('d-none');
            }
        });
    }
}
// end set content col

// set content col modal
function setContentColModal() {
    if($('.contentInputImplementation .rowItem .contentCol').length) {
        $('.contentInputImplementation .rowItem .contentCol').each(function() {
            var contentItem = $(this).html();

            if(!contentItem) $(this).parents('.contentInputImplementation').addClass('fullCol')
        })
    }
}
// end set content col modal

// start select search
function startSelectSearch() {
    if($('.select-search-js').length) {
        $('.select-search-js').each(function(index) {
            var itemSelectSearch = 'select-search-js-'+ index;

            $(this).attr('id', itemSelectSearch);

            const selectSearchJS = new Choices('#'+itemSelectSearch, {
                shouldSort: !1
            });
        });
    }
}
// end start select search

// set collapse file
function setCollapseListFile() {
    if($('.wrapFile').length) {
        $('.wrapFile').each(function(index) {
            var countItem = $(this).find('.linkFile').length;

            if(countItem > 4){
                $(this).addClass('startCollapseFile');
            }
        });
    }
}

$('.btnSeeMoreFile').on('click', function() {
    var heightItem = $(this).parents('.startCollapseFile').find('.wrapCollapseFile .listItem').height();
    $(this).parents('.startCollapseFile').addClass('showAll');
    $(this).parents('.startCollapseFile').find('.wrapCollapseFile').attr('style', 'height: '+ heightItem +'px;');
})

$('.btnHiddenFile').on('click', function() {
    $(this).parents('.startCollapseFile').removeClass('showAll');
    $(this).parents('.startCollapseFile').find('.wrapCollapseFile').attr('style', '');
})
// end set collapse file

// set collapse text
var heiLimitTextCollapse = 140;

function setCollapseText() {
    if($('.wrapExplanation .wrapTextItem').length) {
        $('.wrapExplanation .wrapTextItem').each(function(index) {
            var heightItem = $(this).find('.wrapTextCollapse').height();

            if(heightItem > heiLimitTextCollapse){
                $(this).addClass('startCollapseText');
            }
        });
    }
}

function setCollapseTextModal() {
    if($('.contentFileAndText .wrapExplanation .wrapTextItem').length) {
        $('.contentFileAndText .wrapExplanation .wrapTextItem').each(function(index) {
            var heightItem = $(this).find('.wrapTextCollapse').height();

            if(heightItem > heiLimitTextCollapse){
                $(this).addClass('startCollapseText');
            }
        });
    }
}

$(document).on("click", ".btnSeeMoreText",function() {
    var heightItem = $(this).parents('.startCollapseText').find('.wrapTextCollapse').height();

    $(this).parents('.startCollapseText').addClass('showAll');
    $(this).parents('.startCollapseText').find('.showTextItem').attr('style', 'height: '+ heightItem +'px;');
});

$(document).on("click", ".btnHiddenText",function() {
    $(this).parents('.startCollapseText').removeClass('showAll');
    $(this).parents('.startCollapseText').find('.showTextItem').attr('style', '');
})
// end set collapse text

// show row sub
$('.hasRowSub').on('click', function() {
    var nameItem = $(this).attr('data-row');

    if($(this).hasClass('showRow')) {
        $(this).removeClass('showRow');

        $('.'+ nameItem).removeClass('showRow');
        $('.'+ nameItem).find('.hasRowSub').removeClass('showRow');
        
        $('.children_'+ nameItem).removeClass('showRow');
        $('.children_'+ nameItem).find('.hasRowSub').removeClass('showRow');
    } else {
        $(this).addClass('showRow');
        $('.'+ nameItem).addClass('showRow');

        if($('.showAllRowSub').length) {
            $('.'+ nameItem).find('.hasRowSub').addClass('showRow');

            $('.children_'+ nameItem).addClass('showRow');
            $('.children_'+ nameItem).find('.hasRowSub').addClass('showRow');
        }
    }
})

function showAllRowSub() {
    if($('.rowSub').length && $('.showAllRowSub').length) {
        $('.rowSub, .hasRowSub ').addClass('showRow');
    }
}

function showRowRoot() {
    if($('.level-0').length && $('.showRowRoot').length) {
        $('.level-0').each(function() {
            var dataRow = $(this).find('.hasRowSub').attr('data-row');
            $(this).find('.hasRowSub').addClass('showRow');
            $('.'+ dataRow).addClass('showRow');
        })
    }
}

$('.showAllCriteria').on('click', function() {
    $('.hasRowSub').addClass('showRow');
    $('.rowSub').addClass('showRow');
})

$('.hiddenAllCriteria').on('click', function() {
    $('.hasRowSub').removeClass('showRow');
    $('.rowSub').removeClass('showRow');
})
// end show row sub

// set bg header
function setBgHeader() {
    if($('.navbar-header').length) {
        var imgDS = $('.navbar-header').attr('data-destop');
        var imgMB = $('.navbar-header').attr('data-mobile');

        if($(window).width() > 575) $('.navbar-header').attr('style', 'background-image: url('+ imgDS +')');
        else $('.navbar-header').attr('style', 'background-image: url('+ imgMB +')');
    }
}
// end set bg header

// update text
function ChangeResultCriteria(t) {
    var $this = $(t);
    if ($this.length > 0) {
        if ($this.attr("type") == "radio") {
            var $value = parseFloat($this.val());
            if (!isNaN($value))
                $('.js-modal-score-review').text(format1($value));
            else
                $('.js-modal-score-review').text("---");
        } else if ($this.attr("type") == "checkbox") {
            var $elm = $('#modalImplementationDetail1 .form-check-input[name="checkbox-1"]');
            var $total = 0;
            if ($elm.length > 0) {
                $elm.each(function (index, value) {
                    if ($(value).is(":checked")) {
                        var $score = parseFloat($(value).val());
                        if (!isNaN($score)) {
                            $total += $score;
                        }
                    }
                });
            }
            if ($total > 0) {
                $('.js-modal-score-review').text(format1($total));
            } else {
                $('.js-modal-score-review').text("---");
            }
        }
    }
}
function format2(n) {
    return n.toFixed(2).replace(/./g, function (c, i, a) {
        return i > 0 && c !== "." && (a.length - i) % 3 === 0 ? "," + c : c;
    });
}
function AutoAveragePercent(t) {
    var $this = $(t);
    if ($this.length > 0) {
        var $parent = $this.closest('tbody');
        if ($parent.length > 0) {
            var $numerator = parseFloat($parent.find('.js-number-made').val());
            var $denominator = parseFloat($parent.find('.js-number-total').val());
            if (!isNaN($numerator) && !isNaN($denominator)) {
                $('.js-ty-le-dat-duoc').text(format1($numerator / $denominator * 100) + "%");
            } else
                $('.js-ty-le-dat-duoc').text("");
        }
    }
}
// end update text

setCollapseListFile();
setCollapseText();
showAllRowSub();
showRowRoot();

$(window).on('load', function() {
    fixedButtonCtrl();
    findContentTop();
    setContentCol();
    startSelectSearch();
    setContentColModal();
    setBgHeader();

    var modalImplementationDetail = document.getElementById('modalImplementationDetail');
    if (typeof (modalImplementationDetail) != 'undefined' && modalImplementationDetail != null)
        modalImplementationDetail.addEventListener('shown.bs.modal', function (event) {
            setTimeout(function () {
                setCollapseTextModal();
            }, 200);
        });

    var modalImplementationDetail1 = document.getElementById('modalImplementationDetail1');
    if (typeof (modalImplementationDetail1) != 'undefined' && modalImplementationDetail1 != null)
        modalImplementationDetail1.addEventListener('shown.bs.modal', function (event) {
            setTimeout(function () {
                setCollapseTextModal();
            }, 200);
        });    
});

$(window).on('resize', function() {
    setBgHeader();
});

$(window).on('scroll', function() {
    fixedButtonCtrl();

    if($('.numberFourBtn').hasClass('show') && $('.numberFourBtn').length){
        $('.numberFourBtn').removeClass('show');
    }
});