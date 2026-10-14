<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlProjectTabs.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fProjects.Controls.CtrlProjectTabs" %>

<style>
    /* Thanh ProjectTabs mini: mot hang, thap gon, khong chiem chieu cao noi dung */
    .project-feature-tabs {
        position: relative;
        display: block;
        /* Thu gon vung hover theo chieu rong thuc te cua nhom tab + nut Them,
           khong de ca hang trang kich hoat hieu ung mo rong. */
        width: fit-content !important;
        max-width: 100%;
        overflow-x: auto !important;
        overflow-y: hidden !important;
        margin: 0 auto 8px 0 !important;
        padding: 1px 0 3px !important;
        border: 0 !important;
        outline: none !important;
        border-radius: 0 !important;
        background: transparent !important;
        box-shadow: none !important;
        scrollbar-width: none;
        -ms-overflow-style: none;
        -webkit-overflow-scrolling: touch;
    }

    .project-feature-tabs::-webkit-scrollbar {
        display: none;
        width: 0;
        height: 0;
    }

    .project-feature-tabs .nav-pills {
        display: flex;
        width: max-content;
        max-width: none;
        min-height: 34px;
        align-items: center;
        flex-wrap: nowrap !important;
        gap: 4px;
        margin: 0 !important;
        padding: 0 !important;
        list-style: none;
        border: 0 !important;
        outline: none !important;
        border-radius: 0 !important;
        background: transparent !important;
        box-shadow: none !important;
    }

    .project-feature-tabs .nav-item,
    .project-feature-tabs .project-tab-customize-item {
        flex: 0 0 auto;
        margin: 0 !important;
        padding: 0 !important;
        border: 0 !important;
        background: transparent !important;
        box-shadow: none !important;
    }

    .project-feature-tabs .nav-link {
        position: relative;
        display: inline-flex !important;
        align-items: center;
        justify-content: center;
        min-height: 32px;
        padding: 6px 10px !important;
        border: 1px solid transparent !important;
        border-radius: 8px !important;
        outline: none !important;
        background: transparent !important;
        color: #536174 !important;
        font-size: 12.5px;
        font-weight: 600;
        line-height: 1.1;
        text-decoration: none !important;
        white-space: nowrap;
        -webkit-tap-highlight-color: transparent;
        box-shadow: none !important;
        transition: color .16s ease, background-color .16s ease,
                    border-color .16s ease;
    }

    .project-feature-tabs .nav-link > i {
        margin-right: 6px !important;
        font-size: 12px;
        line-height: 1;
        opacity: .92;
    }

    /* Vong focus ro rang nhung khong tao vien den */
    .project-feature-tabs .nav-link:focus {
        outline: none !important;
    }

    .project-feature-tabs .nav-link:focus-visible {
        outline: none !important;
        box-shadow: 0 0 0 2px rgba(109, 67, 177, .2) !important;
    }

    .project-feature-tabs .nav-link:not(.active):hover {
        color: #3f176f !important;
        background: #e9defb !important;
        border-color: #c9b2ef !important;
        box-shadow: 0 2px 7px rgba(84, 33, 141, .12) !important;
    }

    .project-feature-tabs .nav-link:not(.active):hover > i {
        color: #54218d !important;
        opacity: 1 !important;
    }

    .project-feature-tabs .nav-pills .nav-link.active,
    .project-feature-tabs .nav-pills .show > .nav-link {
        color: #ffffff !important;
        background: #54218d !important;
        border-color: #54218d !important;
        box-shadow: none !important;
        font-weight: 650;
    }

    .project-feature-tabs .nav-pills .nav-link.active:hover {
        color: #ffffff !important;
        background: #481879 !important;
        border-color: #481879 !important;
        transform: none;
        box-shadow: none !important;
    }

    /* Nut bo ghim nho gon, chi hien ro khi hover tab */
    .project-feature-tabs .unpin-btn {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        width: 15px;
        height: 15px;
        margin-left: 5px !important;
        border-radius: 50%;
        color: inherit !important;
        background: rgba(255, 255, 255, .16);
        font-size: 9px;
        line-height: 1;
    }

    .project-feature-tabs .nav-link:not(.active) .unpin-btn {
        background: #ebe3f7;
        color: #65469d !important;
    }

    .project-feature-tabs .nav-link:hover .unpin-btn,
    .project-feature-tabs .nav-link:focus-visible .unpin-btn {
        opacity: 1 !important;
    }

    /* Nut Them dong bo chieu cao voi tab, huy hieu dem rat nho */
    .project-feature-tabs .nav-link.project-tab-customize-link {
        display: inline-flex !important;
        gap: 6px;
        min-height: 32px;
        padding: 5px 8px !important;
        color: #54218d !important;
        background: transparent !important;
        border: 1px solid transparent !important;
        border-radius: 8px !important;
        box-shadow: none !important;
        font-weight: 650;
    }

    .project-feature-tabs .nav-link.project-tab-customize-link:not(.active):hover {
        color: #35105f !important;
        background: #e8dcfb !important;
        border-color: transparent !important;
        box-shadow: none !important;
    }

    .project-feature-tabs .project-tab-customize-icon {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        width: 22px;
        height: 22px;
        flex: 0 0 22px;
        border: 0 !important;
        border-radius: 7px;
        color: #ffffff;
        background: #54218d;
        font-size: 11px;
        line-height: 1;
        transition: color .28s ease, background-color .28s ease,
                    transform .32s ease;
    }

    .project-feature-tabs .project-tab-customize-link:hover .project-tab-customize-icon,
    .project-feature-tabs .project-tab-customize-link:focus-visible .project-tab-customize-icon {
        color: #ffffff;
        background: #3f126f;
        transform: scale(1.06);
    }

    .project-feature-tabs .project-tab-customize-label {
        line-height: 1;
    }

    .project-feature-tabs .project-tab-customize-count {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        min-width: 18px;
        height: 18px;
        padding: 0 4px;
        border: 1px solid #e6daf8;
        border-radius: 6px;
        color: #5b3697;
        background: #ffffff;
        font-size: 10px;
        font-weight: 750;
        font-variant-numeric: tabular-nums;
        line-height: 1;
    }

    #moreTabsMenu .dropdown-item {
        color: #334155;
        font-weight: 500;
        transition: background-color .18s ease, color .18s ease;
    }

    #moreTabsMenu .dropdown-item:hover {
        background-color: #f8fafc;
        color: #1e40af;
    }

    .btn-pin-tab {
        border-radius: 6px;
        font-size: 11.5px;
        padding: 3px 8px;
        font-weight: 700;
        transition: transform .15s ease;
    }

    .btn-pin-tab:hover { transform: translateY(-1px); }

    @media (max-width: 575.98px) {
        .project-feature-tabs { margin-bottom: 7px !important; }
        .project-feature-tabs .nav-pills { gap: 3px; }
        .project-feature-tabs .nav-link { min-height: 31px; padding: 6px 8px !important; font-size: 12px; }
        .project-feature-tabs .nav-link > i { margin-right: 5px !important; }
        .project-feature-tabs .nav-link.project-tab-customize-link { min-height: 31px; gap: 5px; padding: 5px 7px !important; }
        .project-feature-tabs .project-tab-customize-label { display: inline; }
    }

    /* ProjectTabs compact: tab dang chon hien ten, cac tab khac thu gon con icon.
       Khi hover/focus, nhan va bo dem mo rong ngay tren cung mot hang. */
    .project-feature-tabs .nav-pills .nav-link:not(.active):not(.project-tab-customize-link) {
        min-width: 32px;
        padding: 6px 9px !important;
        overflow: hidden;
    }

    .project-feature-tabs .nav-pills .nav-link:not(.active):not(.project-tab-customize-link) > i {
        margin-right: 0 !important;
        flex: 0 0 auto;
    }

    .project-feature-tabs .project-tab-label {
        display: inline-block;
        max-width: 190px;
        overflow: hidden;
        white-space: nowrap;
        opacity: 1;
        vertical-align: middle;
        transition: max-width 1.15s cubic-bezier(.2, .7, .25, 1),
                    opacity .62s ease .10s, margin-left 1.15s cubic-bezier(.2, .7, .25, 1);
    }

    .project-feature-tabs .nav-pills .nav-link:not(.active) > .project-tab-label {
        max-width: 0;
        margin-left: 0;
        opacity: 0;
    }

    .project-feature-tabs .nav-pills .nav-link:not(.active):not(.project-tab-customize-link):hover > .project-tab-label,
    .project-feature-tabs .nav-pills .nav-link:not(.active):not(.project-tab-customize-link):focus-visible > .project-tab-label {
        max-width: 190px;
        margin-left: 6px;
        opacity: 1;
    }

    /* Nut Them mac dinh chi con icon; khi hover hien nhan va so luong chua ghim. */
    .project-feature-tabs .nav-pills .nav-link.project-tab-customize-link {
        min-width: 32px;
        min-height: 32px;
        justify-content: flex-start;
        gap: 0 !important;
        padding: 5px !important;
        overflow: hidden;
        white-space: nowrap;
    }

    .project-feature-tabs .project-tab-customize-icon {
        flex: 0 0 22px;
    }

    .project-feature-tabs .project-tab-customize-label {
        display: inline-block;
        max-width: 0;
        margin-left: 0;
        overflow: hidden;
        opacity: 0;
        white-space: nowrap;
        transition: max-width 1.15s cubic-bezier(.2, .7, .25, 1),
                    opacity .62s ease .10s, margin-left 1.15s cubic-bezier(.2, .7, .25, 1);
    }

    .project-feature-tabs .project-tab-customize-count {
        flex: 0 0 auto;
        width: 0;
        min-width: 0;
        max-width: 0;
        height: 18px;
        margin-left: 0;
        padding: 0;
        border-width: 0;
        overflow: hidden;
        opacity: 0;
        transition: max-width 1.15s cubic-bezier(.2, .7, .25, 1),
                    width 1.15s cubic-bezier(.2, .7, .25, 1),
                    opacity .62s ease .10s, margin-left 1.15s cubic-bezier(.2, .7, .25, 1),
                    padding 1.15s cubic-bezier(.2, .7, .25, 1), border-width 1.15s ease;
    }

    .project-feature-tabs .nav-pills .nav-link.project-tab-customize-link:hover .project-tab-customize-label,
    .project-feature-tabs .nav-pills .nav-link.project-tab-customize-link:focus-visible .project-tab-customize-label {
        max-width: 60px;
        margin-left: 6px;
        opacity: 1;
    }

    .project-feature-tabs .nav-pills .nav-link.project-tab-customize-link:hover .project-tab-customize-count,
    .project-feature-tabs .nav-pills .nav-link.project-tab-customize-link:focus-visible .project-tab-customize-count {
        width: auto;
        min-width: 18px;
        max-width: 28px;
        margin-left: 6px;
        padding: 0 4px;
        border-width: 1px;
        opacity: 1;
    }

    /* Nut bo ghim khong chiem chieu rong khi tab dang thu gon. */
    .project-feature-tabs .unpin-btn {
        display: none !important;
    }

    .project-feature-tabs .nav-pills .nav-link:not(.active):hover .unpin-btn,
    .project-feature-tabs .nav-pills .nav-link:not(.active):focus-visible .unpin-btn {
        display: inline-flex !important;
    }

    /* Mở rộng đồng loạt cả hàng tab khi rê chuột vào vùng thanh điều hướng.
       Không yêu cầu rê đúng từng icon; focus bàn phím cũng mở rộng toàn hàng. */
    .project-feature-tabs:hover .nav-pills .nav-link:not(.active):not(.project-tab-customize-link) > .project-tab-label,
    .project-feature-tabs:focus-within .nav-pills .nav-link:not(.active):not(.project-tab-customize-link) > .project-tab-label {
        max-width: 190px;
        margin-left: 6px;
        opacity: 1;
    }

    .project-feature-tabs:hover .nav-pills .nav-link.project-tab-customize-link .project-tab-customize-label,
    .project-feature-tabs:focus-within .nav-pills .nav-link.project-tab-customize-link .project-tab-customize-label {
        max-width: 60px;
        margin-left: 6px;
        opacity: 1;
    }

    .project-feature-tabs:hover .nav-pills .nav-link.project-tab-customize-link .project-tab-customize-count,
    .project-feature-tabs:focus-within .nav-pills .nav-link.project-tab-customize-link .project-tab-customize-count {
        width: auto;
        min-width: 18px;
        max-width: 28px;
        margin-left: 6px;
        padding: 0 4px;
        border-width: 1px;
        opacity: 1;
    }

    /* Khi hover cả thanh, mở rộng tên theo nhịp chậm và mềm hơn để tránh cảm giác bật đột ngột. */
    .project-feature-tabs:hover .nav-pills .nav-link:not(.active):not(.project-tab-customize-link) > .project-tab-label,
    .project-feature-tabs:focus-within .nav-pills .nav-link:not(.active):not(.project-tab-customize-link) > .project-tab-label,
    .project-feature-tabs:hover .nav-pills .nav-link.project-tab-customize-link .project-tab-customize-label,
    .project-feature-tabs:focus-within .nav-pills .nav-link.project-tab-customize-link .project-tab-customize-label {
        transition: max-width 1.15s cubic-bezier(.2, .7, .25, 1),
                    opacity .62s ease .10s, margin-left 1.15s cubic-bezier(.2, .7, .25, 1);
    }

    .project-feature-tabs:hover .nav-pills .nav-link.project-tab-customize-link .project-tab-customize-count,
    .project-feature-tabs:focus-within .nav-pills .nav-link.project-tab-customize-link .project-tab-customize-count {
        transition: max-width 1.15s cubic-bezier(.2, .7, .25, 1),
                    width 1.15s cubic-bezier(.2, .7, .25, 1), opacity .62s ease .10s,
                    margin-left 1.15s cubic-bezier(.2, .7, .25, 1),
                    padding 1.15s cubic-bezier(.2, .7, .25, 1), border-width 1.15s ease;
    }

    /* Chỉ điều chỉnh kích thước hộp thoại. Cố ý không ghi đè modal-content,
       border, overflow hay vị trí nút đóng của ExtraModal để giữ nguyên viền và nút X. */
    @media (min-width: 992px) {
        #<%= mdlCustomizeTabs.ClientID %> .modal-dialog {
            max-width: 1060px !important;
        }
    }

    /* Bố cục lưới thẻ chức năng: gọn hơn nhưng vẫn dễ bấm */
    .col-custom-5 {
        flex: 0 0 20%;
        max-width: 20%;
        min-width: 0;
    }

    @media (max-width: 1199.98px) {
        .col-custom-5 { flex: 0 0 25%; max-width: 25%; }
    }

    @media (max-width: 991.98px) {
        .col-custom-5 { flex: 0 0 33.333333%; max-width: 33.333333%; }
    }

    @media (max-width: 767.98px) {
        .col-custom-5 { flex: 0 0 50%; max-width: 50%; }
    }

    @media (max-width: 359.98px) {
        .col-custom-5 { flex: 0 0 100%; max-width: 100%; }
    }

    .tabs-customize-intro {
        display: flex;
        align-items: center;
        gap: 10px;
        margin: 0 0 12px;
        padding: 9px 12px;
        color: #526178;
        background: #f7f8fc;
        border: 1px solid #e5e9f2;
        border-radius: 9px;
        font-size: 12px;
        line-height: 1.5;
    }

    .tabs-customize-intro-icon {
        display: inline-flex;
        flex: 0 0 27px;
        align-items: center;
        justify-content: center;
        width: 27px;
        height: 27px;
        color: #5b35a6;
        background: #eee9fb;
        border-radius: 8px;
    }

    #tabsCustomizeContainer {
        align-items: stretch;
    }

    /* Mỗi thẻ là một vùng bấm duy nhất; trạng thái ghim nằm ở góc thẻ. */
    .card-feature-toggle {
        position: relative;
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        width: 100%;
        height: 138px;
        padding: 13px 10px 11px;
        appearance: none;
        -webkit-appearance: none;
        font-family: inherit;
        font-size: inherit;
        line-height: inherit;
        color: #344258;
        text-align: center;
        background: #fff;
        border: 1px solid #e0e6ef;
        border-radius: 10px;
        cursor: pointer;
        outline: none;
        box-shadow: 0 1px 2px rgba(25, 38, 62, .025);
        transition: transform .16s ease, border-color .16s ease,
                    background-color .16s ease, box-shadow .16s ease;
    }

    .card-feature-toggle::before {
        content: "";
        position: absolute;
        top: -1px;
        left: 14px;
        width: 25px;
        height: 3px;
        background: transparent;
        border-radius: 0 0 4px 4px;
        transition: width .16s ease, background-color .16s ease;
    }

    .card-feature-toggle:hover {
        z-index: 1;
        transform: translateY(-2px);
        border-color: #c5b6e7;
        box-shadow: 0 5px 13px rgba(54, 37, 91, .075);
    }

    .card-feature-toggle:focus-visible {
        border-color: #9c82d3;
        box-shadow: 0 0 0 3px rgba(108, 70, 180, .14);
    }

    .card-feature-toggle.is-pinned {
        color: #176f57;
        background: #f3faf7;
        border-color: #bfe5d4;
    }

    .card-feature-toggle.is-pinned::before {
        width: 34px;
        background: #2aa579;
    }

    .card-feature-toggle.is-pinned:hover {
        background: #edf8f2;
        border-color: #94d5b8;
    }

    .card-feature-toggle.is-required {
        cursor: default;
    }

    .card-feature-toggle.is-required:disabled {
        opacity: 1;
        -webkit-text-fill-color: currentColor;
    }

    .card-feature-toggle.is-required:hover {
        transform: none;
        box-shadow: 0 1px 2px rgba(25, 38, 62, .025);
    }

    .feature-card-state {
        position: absolute;
        top: 8px;
        right: 8px;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        width: 21px;
        height: 21px;
        color: #8190a5;
        background: #f3f6fa;
        border: 1px solid #e4eaf2;
        border-radius: 50%;
        font-size: 9px;
        transition: color .16s ease, background-color .16s ease, border-color .16s ease;
    }

    .card-feature-toggle:hover .feature-card-state {
        color: #5d43a0;
        background: #f1ecfb;
        border-color: #ded4f4;
    }

    .card-feature-toggle.is-pinned .feature-card-state,
    .card-feature-toggle.is-pinned:hover .feature-card-state {
        color: #16825d;
        background: #d9f5e7;
        border-color: #b9e9d1;
    }

    .feature-card-icon {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        flex: 0 0 42px;
        width: 42px;
        height: 42px;
        margin-bottom: 9px;
        color: #64758c;
        background: #f0f4f9;
        border: 1px solid #edf1f6;
        border-radius: 12px;
        font-size: 17px;
        transition: color .16s ease, background-color .16s ease, transform .16s ease;
    }

    .card-feature-toggle:hover .feature-card-icon {
        color: #5e459c;
        background: #f0ebfb;
        transform: translateY(-1px);
    }

    .card-feature-toggle.is-pinned .feature-card-icon,
    .card-feature-toggle.is-pinned:hover .feature-card-icon {
        color: #168563;
        background: #def6ea;
        border-color: #d5f0e3;
    }

    .feature-card-name {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 100%;
        min-height: 34px;
        color: #344258;
        font-size: 12.5px;
        font-weight: 650;
        line-height: 1.38;
        overflow-wrap: anywhere;
    }

    .card-feature-toggle.is-pinned .feature-card-name {
        color: #20775e;
    }

    @media (max-width: 767.98px) {
        #<%= mdlCustomizeTabs.ClientID %> .modal-dialog {
            margin: 10px;
        }

        #<%= mdlCustomizeTabs.ClientID %> .modal-body {
            padding: 0;
        }

        .card-feature-toggle {
            height: 132px;
            padding: 12px 8px 10px;
        }

        .tabs-customize-intro {
            align-items: flex-start;
            font-size: 11.5px;
        }
    }

    /* Tab chua duoc chon co nen/ vien nhe; hover dam hon ro rang, khong trung trang thai. */
    .project-feature-tabs .nav-pills .nav-link:not(.active):not(.project-tab-customize-link){color:#536174!important;background:#faf8ff!important;border-color:#e5dcf3!important;box-shadow:0 1px 2px rgba(84,33,141,.035)!important;}
    .project-feature-tabs .nav-pills .nav-link:not(.active):not(.project-tab-customize-link):hover,.project-feature-tabs .nav-pills .nav-link:not(.active):not(.project-tab-customize-link):focus-visible{color:#3f176f!important;background:#e9defb!important;border-color:#c9b2ef!important;box-shadow:0 2px 7px rgba(84,33,141,.12)!important;}

    /* Thanh chức năng trượt xuống khi mở và trượt lên khi thu gọn. */
    .project-feature-tabs{max-height:0!important;max-width:100%;padding:0!important;margin:0!important;opacity:0;transform:translateY(-8px);visibility:hidden;pointer-events:none;overflow-x:auto!important;overflow-y:hidden!important;transition:max-height .38s ease,opacity .22s ease,transform .38s ease,margin .38s ease,padding .38s ease,visibility 0s linear .38s;}
    .project-feature-tabs.project-tabs-open{max-height:56px!important;padding:1px 0 3px!important;margin:0 auto 8px 0!important;opacity:1;transform:translateY(0);visibility:visible;pointer-events:auto;transition:max-height .38s ease,opacity .24s ease,transform .38s ease,margin .38s ease,padding .38s ease,visibility 0s;}
    @media(prefers-reduced-motion:reduce){.project-feature-tabs,.project-feature-tabs.project-tabs-open{transition:none;}}
    /* Hien day du ten tat ca chuc nang ngay ca khi chua hover. */
    .project-feature-tabs .nav-pills .nav-link:not(.active):not(.project-tab-customize-link){min-width:fit-content!important;padding:6px 10px!important;overflow:visible!important;}
    .project-feature-tabs .nav-pills .nav-link:not(.active):not(.project-tab-customize-link)>i{margin-right:6px!important;}
    .project-feature-tabs .project-tab-label,.project-feature-tabs .nav-pills .nav-link:not(.active)>.project-tab-label{display:inline-block!important;max-width:none!important;width:auto!important;margin-left:6px!important;overflow:visible!important;opacity:1!important;white-space:nowrap!important;}
    .project-feature-tabs .nav-pills .nav-link.project-tab-customize-link{min-width:fit-content!important;gap:6px!important;padding:5px 8px!important;overflow:visible!important;}
    .project-feature-tabs .project-tab-customize-label{display:inline-block!important;max-width:none!important;width:auto!important;margin-left:0!important;overflow:visible!important;opacity:1!important;white-space:nowrap!important;}
    .project-feature-tabs .project-tab-customize-count{width:auto!important;min-width:18px!important;max-width:none!important;height:18px!important;margin-left:0!important;padding:0 4px!important;border-width:1px!important;overflow:visible!important;opacity:1!important;}
    @media(max-width:575.98px){.project-feature-tabs .nav-pills .nav-link:not(.active):not(.project-tab-customize-link){padding:6px 8px!important;}.project-feature-tabs .nav-pills .nav-link.project-tab-customize-link{gap:5px!important;padding:5px 7px!important;}}
    /* Dong bo nut Them voi cac tab chua chon: co vien ngoai, nen nhe va chu cung co chu. */
    .project-feature-tabs .nav-pills .nav-link.project-tab-customize-link{min-height:32px!important;gap:7px!important;padding:5px 9px!important;color:#54218d!important;background:#faf8ff!important;border:1px solid #e5dcf3!important;border-radius:8px!important;box-shadow:0 1px 2px rgba(84,33,141,.035)!important;font-size:12.5px!important;font-weight:650!important;line-height:1.1!important;}
    .project-feature-tabs .nav-pills .nav-link.project-tab-customize-link:hover,.project-feature-tabs .nav-pills .nav-link.project-tab-customize-link:focus-visible{color:#3f176f!important;background:#e9defb!important;border-color:#c9b2ef!important;box-shadow:0 2px 7px rgba(84,33,141,.12)!important;}
    .project-feature-tabs .project-tab-customize-label{font-size:12.5px!important;font-weight:650!important;line-height:1.1!important;}
    .project-feature-tabs .project-tab-customize-count{min-width:21px!important;height:20px!important;padding:0 5px!important;border:1px solid #e6daf8!important;border-radius:6px!important;font-size:12.5px!important;font-weight:750!important;line-height:1!important;}
    @media(max-width:575.98px){.project-feature-tabs .nav-pills .nav-link.project-tab-customize-link{min-height:31px!important;gap:5px!important;padding:5px 7px!important;font-size:12px!important;}.project-feature-tabs .project-tab-customize-label{font-size:12px!important;}.project-feature-tabs .project-tab-customize-count{min-width:20px!important;height:19px!important;font-size:12px!important;}}
</style>

<nav id="projectFeatureTabs" class="project-feature-tabs mb-3" aria-label="Chức năng trong dự án">
    <ul runat="server" id="ulProjectTabs" class="nav nav-pills align-items-center"></ul>
</nav>

<!-- POPUP TÙY CHỈNH CHỨC NĂNG HIỂN THỊ -->
<SweetSoft:ExtraModal ID="mdlCustomizeTabs" Type="Primary" Title="Tùy chỉnh chức năng hiển thị" runat="server">
    <ContentTemplate>
        <div class="p-3">
            <div class="tabs-customize-intro">
                <span class="tabs-customize-intro-icon"><i class="fas fa-sliders-h"></i></span>
                <span>Nhấn vào thẻ chức năng để ghim hoặc bỏ ghim khỏi thanh công cụ. Mặc định 3 chức năng đầu tiên được ghim ở lần truy cập đầu; luôn giữ lại chức năng của trang đang mở.</span>
            </div>
            <div id="tabsCustomizeContainer" class="row g-2"></div>
        </div>
    </ContentTemplate>
    <FooterTemplate>
        <button type="button" class="btn btn-primary" onclick="saveTabPreferences()"><i class="fas fa-save me-1"></i> Lưu thay đổi</button>
    </FooterTemplate>
</SweetSoft:ExtraModal>

<script type="text/javascript">
    var allTabs = [];
    var defaultPinnedKeys = [];
    var activeKey = null;
    var pinnedKeys = [];
    var tempPinnedKeys = [];

    $(function () {
        var $ul = $('#<%= ulProjectTabs.ClientID %>');
        if ($ul.length === 0) return;

        // Thu thập các chức năng do server sinh ra.
        allTabs = [];
        defaultPinnedKeys = [];
        activeKey = null;

        $ul.find('li[data-module]').each(function (index) {
            var $li = $(this);
            var key = $li.attr('data-module');
            var $a = $li.find('a').first();
            var isActive = $a.hasClass('active');
            var name = $a.clone().children().remove().end().text().trim();
            var icon = $li.find('i').first().attr('class') || 'fas fa-folder';
            var href = $a.attr('href');

            // Gom text node do server render vao span de CSS co the an/hien label theo hover.
            // Chi lay text node, khong sua icon hay href cua tab.
            var textNodes = $a.contents().filter(function () {
                return this.nodeType === 3 && $.trim(this.nodeValue).length > 0;
            });
            if (textNodes.length) {
                var labelText = textNodes.map(function () { return this.nodeValue; }).get().join(' ').trim();
                textNodes.remove();
                $('<span class="project-tab-label"></span>').text(labelText).appendTo($a);
            }

            allTabs.push({ key: key, name: name, icon: icon, href: href, isActive: isActive, $el: $li });

            if (isActive) activeKey = key;
            if (index < 3) defaultPinnedKeys.push(key);
        });

        // Lần đầu truy cập: ghim 3 chức năng đầu. Những lần sau dùng cấu hình đã lưu.
        var saved = localStorage.getItem('SweetSoft_PinnedProjectTabs_v4');
        if (saved) {
            try {
                var parsed = JSON.parse(saved);
                pinnedKeys = Array.isArray(parsed) ? parsed : defaultPinnedKeys.slice();
            } catch (error) {
                pinnedKeys = defaultPinnedKeys.slice();
            }
        } else {
            pinnedKeys = defaultPinnedKeys.slice();
        }

        // Loại bỏ các khóa không còn tồn tại, tránh cấu hình cũ làm lệch số lượng.
        var availableKeys = allTabs.map(function (tab) { return tab.key; });
        pinnedKeys = pinnedKeys.filter(function (key, index, list) {
            return availableKeys.indexOf(key) !== -1 && list.indexOf(key) === index;
        });

        // Luôn giữ tab đang xem để người dùng không mất dấu trang hiện tại.
        if (activeKey && pinnedKeys.indexOf(activeKey) === -1) {
            pinnedKeys.push(activeKey);
        }

        // Nếu cấu hình rỗng hoặc không còn tab hợp lệ, giữ lại ít nhất một chức năng.
        if (pinnedKeys.length === 0 && allTabs.length > 0) {
            pinnedKeys.push(activeKey || allTabs[0].key);
        }

        // Sắp xếp thanh tab theo thứ tự server khai báo, chỉ hiển thị các tab được ghim.
        $ul.empty();
        var unpinnedCount = 0;

        allTabs.forEach(function (tab) {
            if (pinnedKeys.indexOf(tab.key) !== -1) {
                var $li = tab.$el;

                if (tab.key !== activeKey) {
                    var $a = $li.find('a').first();
                    var $unpinBtn = $('<span class="ms-2 text-muted unpin-btn" title="Bỏ ghim" style="cursor:pointer;opacity:0;transition:opacity .18s;"><i class="fas fa-times"></i></span>');
                    $a.find('.unpin-btn').remove();
                    $a.append($unpinBtn);
                    $a.hover(
                        function () { $unpinBtn.css('opacity', '1'); },
                        function () { $unpinBtn.css('opacity', '0'); }
                    );

                    $unpinBtn.on('click', function (e) {
                        e.preventDefault();
                        e.stopPropagation();

                        // Khong cho bo ghim tab dang mo hoac tab cuoi cung.
                        if (pinnedKeys.length <= 1 || tab.key === activeKey) return;

                        // Cap nhat trang thai ngay tren client, khong tai lai ca trang.
                        pinnedKeys = pinnedKeys.filter(function (key) { return key !== tab.key; });
                        localStorage.setItem('SweetSoft_PinnedProjectTabs_v4', JSON.stringify(pinnedKeys));

                        // An tab vua bo ghim; van giu doi tuong trong allTabs de popup co the them lai.
                        if (tab.$el && tab.$el.length) {
                            tab.$el.stop(true, true).remove();
                        }

                        updateCustomizeButtonCount();
                    });
                }
                $ul.append($li);
            } else {
                unpinnedCount++;
            }
        });

        // Nút mở popup để thêm/bớt chức năng trên thanh tab.
        // Bộ đếm đặt trong huy hiệu riêng, tránh cảm giác nút bị chia đôi/viền nối.
        var customizeTitle = 'Tùy chỉnh chức năng hiển thị (' + unpinnedCount + ' chức năng chưa ghim)';
        var $btnCustomize = $('<li class="nav-item project-tab-customize-item">' +
            '<a class="nav-link project-tab-customize-link" href="javascript:;" onclick="openCustomizeModal()" title="' + customizeTitle + '" aria-label="' + customizeTitle + '">' +
            '<span class="project-tab-customize-icon" aria-hidden="true"><i class="fas fa-plus"></i></span>' +
            '<span class="project-tab-customize-label">Thêm</span>' +
            '<span class="project-tab-customize-count">' + unpinnedCount + '</span>' +
            '</a></li>');
        $ul.append($btnCustomize);
    });

    // Cap nhat so chuc nang chua ghim ngay tren nut +, khong can reload trang.
    function updateCustomizeButtonCount() {
        var unpinnedCount = Math.max(0, allTabs.length - pinnedKeys.length);
        var title = 'Tùy chỉnh chức năng hiển thị (' + unpinnedCount + ' chức năng chưa ghim)';
        var $button = $('.project-feature-tabs .project-tab-customize-link');

        if (!$button.length) return;

        $button.find('.project-tab-customize-count').text(unpinnedCount);
        $button.attr('title', title).attr('aria-label', title);
    }

    function openCustomizeModal() {
        tempPinnedKeys = pinnedKeys.slice();
        renderCustomizeCards();
        $('#<%= mdlCustomizeTabs.ClientID %>').modal('show');
    }

    function closeCustomizeModal() {
        $('#<%= mdlCustomizeTabs.ClientID %>').modal('hide');
    }

    function escapeCustomizeHtml(value) {
        return $('<div/>').text(value == null ? '' : String(value)).html();
    }

    // Vẽ thẻ chức năng. Trạng thái được thể hiện bằng màu nền/viền và dấu góc,
    // không còn nút "Bật/Đang bật" chiếm chỗ phía dưới tên chức năng.
    function renderCustomizeCards() {
        var $container = $('#tabsCustomizeContainer');
        $container.empty();

        allTabs.forEach(function (tab) {
            var isPinned = tempPinnedKeys.indexOf(tab.key) !== -1;
            // Chỉ khóa chức năng của trang đang mở; các chức năng mặc định khác có thể bỏ ghim.
            var isRequired = tab.key === activeKey;
            var cardClasses = 'card-feature-toggle' + (isPinned ? ' is-pinned' : ' is-unpinned') + (isRequired ? ' is-required' : '');
            var stateIcon = isPinned ? 'fa-check' : 'fa-plus';
            var title = isRequired
                ? 'Chức năng đang mở luôn được giữ trên thanh công cụ'
                : (isPinned ? 'Nhấn để bỏ ghim chức năng này' : 'Nhấn để ghim chức năng này');
            var disabledAttr = isRequired ? 'disabled="disabled"' : '';
            var iconClass = escapeCustomizeHtml(tab.icon || 'fas fa-folder');
            var tabName = escapeCustomizeHtml(tab.name);
            var safeKey = escapeCustomizeHtml(tab.key);

            var html = '' +
                '<div class="col-custom-5">' +
                '<button type="button" class="' + cardClasses + '" data-module="' + safeKey + '" title="' + escapeCustomizeHtml(title) + '" aria-pressed="' + (isPinned ? 'true' : 'false') + '" ' + disabledAttr + '>' +
                '<span class="feature-card-state" aria-hidden="true"><i class="fas ' + stateIcon + '"></i></span>' +
                '<span class="feature-card-icon" aria-hidden="true"><i class="' + iconClass + '"></i></span>' +
                '<span class="feature-card-name">' + tabName + '</span>' +
                '</button>' +
                '</div>';

            var $card = $(html);
            if (!isRequired) {
                $card.find('.card-feature-toggle').on('click', function () {
                    togglePin(tab.key);
                });
            }
            $container.append($card);
        });
    }

    // Không cho tắt tab đang mở hoặc tắt chức năng cuối cùng còn ghim.
    function togglePin(key) {
        var idx = tempPinnedKeys.indexOf(key);

        if (idx !== -1) {
            if (key === activeKey || tempPinnedKeys.length <= 1) return;
            tempPinnedKeys.splice(idx, 1);
        } else {
            tempPinnedKeys.push(key);
        }

        renderCustomizeCards();
    }

    function saveTabPreferences() {
        // Chốt bảo vệ tối thiểu một chức năng, kể cả khi cấu hình bị thay đổi ngoài UI.
        if (tempPinnedKeys.length === 0 && allTabs.length > 0) {
            tempPinnedKeys.push(activeKey || allTabs[0].key);
        }

        // Tab hiện tại luôn được giữ lại.
        if (activeKey && tempPinnedKeys.indexOf(activeKey) === -1) {
            tempPinnedKeys.push(activeKey);
        }

        pinnedKeys = tempPinnedKeys.slice();
        localStorage.setItem('SweetSoft_PinnedProjectTabs_v4', JSON.stringify(pinnedKeys));
        closeCustomizeModal();
        location.reload();
    }
</script>
