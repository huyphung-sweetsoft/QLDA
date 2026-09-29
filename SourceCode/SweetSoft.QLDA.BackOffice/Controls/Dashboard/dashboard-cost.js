(function () {
    "use strict";

    var texts = window.dashboardCostTexts || {};

    function showEmptyState(element, message, minimumHeight) {
        if (!element) {
            return;
        }

        element.innerHTML =
            '<div class="d-flex align-items-center justify-content-center text-muted cost-chart-empty"' +
            ' style="min-height:' + (minimumHeight || 300) + 'px">' +
            message +
            '</div>';
    }

    function formatMoney(value) {
        var amount = Number(value) || 0;
        var absoluteAmount = Math.abs(amount);
        var suffix = texts.currencySuffix || "";
        var locale = texts.locale || undefined;

        if (absoluteAmount >= 1000000000) {
            return (amount / 1000000000).toLocaleString(locale, {
                maximumFractionDigits: 2
            }) + (texts.billionSuffix || "");
        }

        if (absoluteAmount >= 1000000) {
            return (amount / 1000000).toLocaleString(locale, {
                maximumFractionDigits: 1
            }) + (texts.millionSuffix || "");
        }

        return amount.toLocaleString(locale, {
            maximumFractionDigits: 0
        }) + suffix;
    }

    function renderProjectComparisonChart() {
        var element = document.getElementById("cost-project-comparison-chart");
        var data = window.dashboardCostProjectData || [];
        if (!element || typeof ApexCharts === "undefined") { return; }
        if (data.length === 0) {
            showEmptyState(element, texts.noCostItems || "", 260);
            return;
        }

        var chartHeight = Math.max(260, data.length * 48 + 55);
        element.style.cursor = "pointer";
        new ApexCharts(element, {
            chart: {
                type: "bar",
                height: chartHeight,
                toolbar: { show: false },
                events: {
                    dataPointSelection: function (event, context, config) {
                        var item = data[config.dataPointIndex];
                        if (item) {
                            openCostModal("costApprovedItemsModal",
                                (texts.approvedCost || "") + " · " + item.code,
                                { projectId: item.projectId });
                        }
                    }
                }
            },
            series: [{
                name: texts.approvedCost || "",
                data: data.map(function (item) { return Number(item.actualCost) || 0; })
            }],
            colors: ["#ef6b72"],
            plotOptions: {
                bar: { horizontal: true, barHeight: "52%", borderRadius: 4 }
            },
            xaxis: {
                min: 0,
                categories: data.map(function (item) { return item.code; }),
                labels: { formatter: formatMoney }
            },
            dataLabels: { enabled: false },
            legend: { show: false },
            tooltip: {
                x: {
                    formatter: function (value, options) {
                        var item = data[options.dataPointIndex];
                        return item ? item.code + " · " + item.name : value;
                    }
                },
                y: { formatter: formatMoney }
            },
            states: { active: { filter: { type: "none" } } }
        }).render();
    }
    function renderPaymentChart() {
        var element = document.getElementById("cost-payment-chart");
        var data = window.dashboardCostPaymentData || {
            received: 0,
            outstanding: 0
        };
        var received = Number(data.received) || 0;
        var outstanding = Number(data.outstanding) || 0;
        var paymentTotal = received + outstanding;
        var total = Number(data.totalContractValue);
        if (!isFinite(total)) {
            total = paymentTotal;
        }

        if (!element || typeof ApexCharts === "undefined") {
            return;
        }

        if (paymentTotal === 0) {
            showEmptyState(element, texts.noContractOrPayment || "", 300);
            return;
        }

        element.style.cursor = "pointer";
        new ApexCharts(element, {
            chart: {
                type: "donut",
                height: 300,
                toolbar: { show: false },
                events: {
                    dataPointSelection: function (event, context, config) {
                        var filter = config.dataPointIndex === 0
                            ? "received" : "outstanding";
                        openCostModal("costPaymentProjectsModal",
                            filter === "received" ? texts.received : texts.outstanding,
                            { paymentFilter: filter });
                    }
                }
            },
            labels: [texts.received || "", texts.outstanding || ""],
            series: [received, outstanding],
            colors: ["#34c38f", "#f1b44c"],
            legend: {
                position: "bottom",
                formatter: function (name, options) {
                    return name + ": " + formatMoney(options.w.globals.series[options.seriesIndex]);
                }
            },
            dataLabels: {
                enabled: true,
                formatter: function (percentage) {
                    return percentage > 0 && percentage < 1
                        ? "<1%" : Math.round(percentage) + "%";
                },
                style: { fontSize: "12px", fontWeight: 700, colors: ["#18273f"] },
                dropShadow: { enabled: false }
            },
            tooltip: {
                y: {
                    formatter: function (value) {
                        return formatMoney(value);
                    }
                }
            },
            plotOptions: {
                pie: {
                    expandOnClick: false,
                    dataLabels: { minAngleToShowLabel: 8 },
                    donut: {
                        size: "62%",
                        labels: {
                            show: true,
                            total: {
                                show: true,
                                label: texts.contractValue || "",
                                formatter: function () {
                                    return formatMoney(total);
                                }
                            }
                        }
                    }
                }
            },
            states: {
                active: { filter: { type: "none" } }
            }
        }).render();
    }

    function renderApprovalChart() {
        var element = document.getElementById("cost-approval-chart");
        var data = window.dashboardCostApprovalData || { approved: 0, pending: 0 };
        var approved = Number(data.approved) || 0;
        var pending = Number(data.pending) || 0;
        if (!element || typeof ApexCharts === "undefined") { return; }
        if (approved + pending === 0) {
            showEmptyState(element, texts.noCostItems || "", 300);
            return;
        }

        element.style.cursor = "pointer";
        new ApexCharts(element, {
            chart: {
                type: "donut",
                height: 300,
                toolbar: { show: false },
                events: {
                    dataPointSelection: function (event, context, config) {
                        var approvedSlice = config.dataPointIndex === 0;
                        openCostModal(
                            approvedSlice ? "costApprovedItemsModal" : "costPendingApprovalModal",
                            approvedSlice ? texts.approvedCost : texts.pendingCost);
                    }
                }
            },
            labels: [texts.approvedCost || "", texts.pendingCost || ""],
            series: [approved, pending],
            colors: ["#ef6b72", "#f1b44c"],
            legend: {
                position: "bottom",
                formatter: function (name, options) {
                    return name + ": " + formatMoney(options.w.globals.series[options.seriesIndex]);
                }
            },
            dataLabels: {
                enabled: true,
                formatter: function (percentage) {
                    return percentage > 0 && percentage < 1
                        ? "<1%" : Math.round(percentage) + "%";
                },
                style: { fontSize: "12px", fontWeight: 700, colors: ["#18273f"] },
                dropShadow: { enabled: false }
            },
            tooltip: { y: { formatter: formatMoney } },
            plotOptions: {
                pie: {
                    expandOnClick: false,
                    dataLabels: { minAngleToShowLabel: 8 },
                    donut: {
                        size: "62%",
                        labels: {
                            show: true,
                            total: {
                                show: true,
                                label: texts.totalRecordedCost || "",
                                formatter: function () { return formatMoney(approved + pending); }
                            }
                        }
                    }
                }
            },
            states: { active: { filter: { type: "none" } } }
        }).render();
    }
    function openCostModal(id, title, filter) {
        var modal = document.getElementById(id);
        if (!modal || !window.bootstrap || !window.bootstrap.Modal) { return; }
        modal.dataset.costModalTitle = title || "";
        modal.dataset.costProjectId = filter && filter.projectId || "";
        modal.dataset.costPaymentFilter = filter && filter.paymentFilter || "";
        window.bootstrap.Modal.getOrCreateInstance(modal).show();
    }

    function bindDashboardListSearch() {
        var dashboard = document.querySelector(".dashboard-cost");
        if (!dashboard) {
            return;
        }

        dashboard.querySelectorAll("[data-dashboard-list-search]")
            .forEach(function (input) {
                var listId = input.getAttribute(
                    "data-dashboard-list-search");
                var listBody = document.getElementById(listId);
                if (!listBody) {
                    return;
                }

                var rows = Array.prototype.slice.call(
                    listBody.querySelectorAll("[data-search-row]")
                );
                var emptyRow = listBody.querySelector("[data-search-empty]");
                var groups = window.DashboardProjectGroups
                    ? window.DashboardProjectGroups.create(listBody) : null;

                function applySearch() {
                    var query = (input.value || "")
                        .trim()
                        .toLocaleLowerCase();
                    var visibleCount = 0;

                    rows.forEach(function (row) {
                        var matches = !query || row.textContent
                            .toLocaleLowerCase()
                            .indexOf(query) >= 0;
                        var modal = input.closest(".modal");
                        var projectId = modal && modal.dataset.costProjectId;
                        var paymentFilter = modal && modal.dataset.costPaymentFilter;
                        if (projectId && row.getAttribute("data-cost-project-id") !== projectId) {
                            matches = false;
                        }
                        if (paymentFilter && row.getAttribute("data-cost-" + paymentFilter) !== "1") {
                            matches = false;
                        }
                        row.classList.toggle("d-none", !matches);
                        if (matches) {
                            visibleCount += 1;
                        }
                    });

                    if (emptyRow && rows.length > 0) {
                        emptyRow.classList.toggle(
                            "d-none",
                            visibleCount > 0);
                    }
                    if (groups) {
                        groups.refresh(Boolean(query));
                    }
                }

                input.addEventListener("input", applySearch);
            });

        dashboard.querySelectorAll(".modal").forEach(function (modal) {
            modal.addEventListener("show.bs.modal", function (event) {
                var trigger = event.relatedTarget;
                var title = modal.querySelector(".modal-title");
                var search = modal.querySelector(
                    "[data-dashboard-list-search]");

                modal.dataset.costProjectId = trigger
                    ? "" : (modal.dataset.costProjectId || "");
                modal.dataset.costPaymentFilter = trigger
                    ? (trigger.getAttribute("data-cost-payment-filter") || "")
                    : (modal.dataset.costPaymentFilter || "");
                if (title) {
                    title.textContent = (trigger && trigger.getAttribute("data-cost-modal-title"))
                        || modal.dataset.costModalTitle || title.textContent;
                }

                if (search) {
                    var listBody = document.getElementById(
                        search.getAttribute("data-dashboard-list-search"));
                    if (listBody && listBody.__dashboardProjectGroups) {
                        listBody.__dashboardProjectGroups.reset();
                    }
                    search.value = "";
                    search.dispatchEvent(new Event("input", {
                        bubbles: true
                    }));
                }
            });
            modal.addEventListener("hidden.bs.modal", function () {
                modal.dataset.costProjectId = "";
                modal.dataset.costPaymentFilter = "";
                modal.dataset.costModalTitle = "";
            });
        });
    }

    document.addEventListener("DOMContentLoaded", function () {
        renderProjectComparisonChart();
        renderPaymentChart();
        renderApprovalChart();
        bindDashboardListSearch();
    });
})();
