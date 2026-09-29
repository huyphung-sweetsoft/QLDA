function escapeDashboardHtml(value) {
    return String(value == null ? "" : value).replace(/[&<>"']/g, function (character) {
        return {
            "&": "&amp;",
            "<": "&lt;",
            ">": "&gt;",
            "\"": "&quot;",
            "'": "&#39;"
        }[character];
    });
}

function renderProjectStatusChart() {

    var element = document.getElementById("project-status-chart");

    if (!element) {
        return;
    }

    if (typeof ApexCharts === "undefined") {
        console.error("ApexCharts is unavailable.");
        return;
    }

    if (!window.projectStatusChartData) {
        console.error("projectStatusChartData is unavailable.");
        return;
    }

    var data = window.projectStatusChartData;

    var chart = new ApexCharts(element, {

        chart: {
            type: "donut",
            height: 320,
            width: "100%",
            toolbar: {
                show: false
            }
        },
        plotOptions: {
            pie: {
                expandOnClick: false
            }
        },
        states: {
            active: { filter: { type: "none" } }
        },

        labels: data.labels,

        series: data.values,

        legend: {
            position: "bottom"
        },

        dataLabels: {
            enabled: true,
            formatter: function (value, opts) {

                var series = opts.w.config.series;

                var total = series.reduce(function (sum, item) {
                    return sum + Number(item || 0);
                }, 0);

                if (total === 0) {
                    return "0.0%";
                }

                var currentValue =
                    Number(series[opts.seriesIndex] || 0);

                return ((currentValue / total) * 100).toFixed(1) + "%";
            }
        }
    });

    chart.render();
}
function renderProjectProgressChart() {

    var element = document.getElementById("project-progress-chart");

    if (!element) {
        return;
    }

    if (typeof ApexCharts === "undefined") {
        console.error("ApexCharts is unavailable.");
        return;
    }

    if (!window.projectProgressChartData) {
        console.error("projectProgressChartData is unavailable.");
        return;
    }

    var data = window.projectProgressChartData;
    var texts = window.dashboardOverviewTexts || {};

    var chartHeight = 330;
    var chartWrapper = document.getElementById("project-progress-chart-wrapper");
    var visibleProjectCount = 6;
    var viewportWidth = chartWrapper ? chartWrapper.clientWidth : 0;

    if (viewportWidth <= 0) {
        viewportWidth = element.parentElement.clientWidth || 900;
    }

    var chartWidth = data.length > visibleProjectCount
        ? Math.ceil(viewportWidth * data.length / visibleProjectCount)
        : viewportWidth;

    if (data.length === 0) {
        element.style.height = chartHeight + "px";
        element.style.cursor = "default";
        element.innerHTML = '<div class="text-center text-muted py-5">'
            + escapeDashboardHtml(texts.noProgressData || "")
            + "</div>";
        return;
    }

    element.style.height = chartHeight + "px";
    element.style.width = chartWidth + "px";
    element.style.cursor = data.some(function (item) {
        return item && item.detailUrl;
    }) ? "pointer" : "default";

    var options = {

        chart: {
            type: "bar",
            height: chartHeight,
            width: chartWidth,
            toolbar: {
                show: false
            },
            parentHeightOffset: 0
        },
        colors: ["#c6ceda", "#4a148c"],
        grid: {
            borderColor: "#e9edf3",
            strokeDashArray: 4,
            padding: {
                top: 20,
                right: 12,
                bottom: 12,
                left: 12
            }
        },

        series: [
            {
                name: texts.plannedProgress || "",
                data: data.map(function (item) {
                    return Number(item.plannedProgress || 0);
                })
            },
            {
                name: texts.progress || "",
                data: data.map(function (item) {
                    return Number(item.progress);
                })
            }
        ],

        xaxis: {
            categories: data.map(function (item) {
                return item.code || item.name || "";
            }),
            labels: {
                rotate: data.length > 6 ? -40 : 0,
                hideOverlappingLabels: true,
                style: {
                    colors: ["#475467"],
                    fontSize: "12px",
                    fontWeight: 500
                }
            }
        },

        yaxis: {
            min: 0,
            max: 100,
            tickAmount: 5,
            labels: {
                formatter: function (value) {
                    return Math.round(Number(value)) + "%";
                },
                style: {
                    colors: ["#667085"],
                    fontSize: "11px"
                }
            },
            title: {
                text: texts.progressAxis || ""
            }
        },

        plotOptions: {
            bar: {
                horizontal: false,
                columnWidth: "52%",
                borderRadius: 4,
                borderRadiusApplication: "end",
                dataLabels: {
                    position: "top"
                }
            }
        },

        dataLabels: {
            enabled: true,
            offsetY: -18,
            formatter: function (val, opts) {
                return Number(val).toFixed(0) + "%";
            },
            style: {
                colors: ["#667085", "#4a148c"],
                fontSize: "10px",
                fontWeight: 600
            }
        },

        legend: {
            show: true,
            position: "top",
            horizontalAlign: "right",
            fontSize: "12px",
            labels: {
                colors: "#475467"
            },
            markers: {
                width: 10,
                height: 10,
                radius: 3
            },
            itemMargin: {
                horizontal: 10
            }
        },

        tooltip: {
            enabled: false
        }
    };

    var chart = new ApexCharts(element, options);
    chart.render();

    // CUSTOM TOOLTIP APPENDED TO BODY TO AVOID CLIPPING
    var customTooltip = document.createElement('div');
    customTooltip.id = 'custom-apex-tooltip';
    customTooltip.style.position = 'absolute';
    customTooltip.style.display = 'none';
    customTooltip.style.zIndex = '9999';
    customTooltip.style.pointerEvents = 'none';
    customTooltip.style.transition = 'opacity 0.2s';
    customTooltip.style.opacity = '0';
    document.body.appendChild(customTooltip);

    element.addEventListener('mousemove', function (e) {
        if (customTooltip.style.display === 'block') {
            // Position near cursor
            var x = e.pageX + 15;
            var y = e.pageY + 15;

            // Prevent going off-screen
            var tooltipRect = customTooltip.getBoundingClientRect();
            if (x + tooltipRect.width > window.innerWidth + window.scrollX) {
                x = e.pageX - tooltipRect.width - 15;
            }
            if (y + tooltipRect.height > window.innerHeight + window.scrollY) {
                y = e.pageY - tooltipRect.height - 15;
            }

            customTooltip.style.left = x + 'px';
            customTooltip.style.top = y + 'px';
        }
    });

    element.addEventListener('mouseout', function () {
        customTooltip.style.opacity = '0';
        setTimeout(function() {
            if (customTooltip.style.opacity === '0') customTooltip.style.display = 'none';
        }, 200);
    });

    // We need to hook into ApexCharts events to know WHICH data point we hovered
    options.chart.events = {
        dataPointMouseEnter: function (event, chartContext, config) {
            var item = data[config.dataPointIndex];
            if (!item) return;

            var healthReasonHtml = item.healthReason
                ? '<div style="margin:-2px 0 7px; color:#667085;">'
                    + escapeDashboardHtml(item.healthReason)
                    + '</div>'
                : '';

            customTooltip.innerHTML = `
            <div style="
                padding:10px 12px;
                min-width:240px;
                background:#fff;
                border:1px solid #e5e7eb;
                border-radius:6px;
                box-shadow:0 4px 12px rgba(0,0,0,.12);
                font-family: inherit;
                font-size: 13px;
                color: #333;
            ">
                <div style="font-weight:600; margin-bottom:8px;">
                    ${escapeDashboardHtml(item.code)} · ${escapeDashboardHtml(item.name)}
                </div>
                <div style="margin-bottom:6px;">
                    <span class="badge ${escapeDashboardHtml(item.healthCss || "")}">${escapeDashboardHtml(item.health || "")}</span>
                </div>
                ${healthReasonHtml}
                <div>
                    <strong>${escapeDashboardHtml(texts.progress || "")}:</strong>
                    ${Number(item.progress).toFixed(1)}%
                </div>
                <div>
                    <strong>${escapeDashboardHtml(texts.plannedProgress || "")}:</strong>
                    ${Number(item.plannedProgress).toFixed(1)}%
                </div>
                <div>
                    <strong>${escapeDashboardHtml(texts.completedTasks || "")}:</strong>
                    ${Number(item.completedTaskCount || 0)}/${Number(item.taskCount || 0)}
                </div>
                <div>
                    <strong>${escapeDashboardHtml(texts.start || "")}:</strong>
                    ${escapeDashboardHtml(item.startDate)}
                </div>
                <div>
                    <strong>${escapeDashboardHtml(texts.expected || "")}:</strong>
                    ${escapeDashboardHtml(item.expectedEndDate)}
                </div>
            </div>`;

            customTooltip.style.display = 'block';
            // Force reflow
            void customTooltip.offsetWidth;
            customTooltip.style.opacity = '1';
        },
        dataPointMouseLeave: function (event, chartContext, config) {
            customTooltip.style.opacity = '0';
            setTimeout(function() {
                if (customTooltip.style.opacity === '0') customTooltip.style.display = 'none';
            }, 200);
        },
        dataPointSelection: function (event, chartContext, config) {
            var item = data[config.dataPointIndex];

            if (item && item.detailUrl) {
                window.location.assign(item.detailUrl);
            }
        }
    };
    chart.updateOptions(options);

    var resizeTimer;
    window.addEventListener("resize", function () {
        window.clearTimeout(resizeTimer);
        resizeTimer = window.setTimeout(function () {
            var updatedViewportWidth = chartWrapper
                ? chartWrapper.clientWidth
                : 0;
            if (updatedViewportWidth <= 0) {
                return;
            }

            var updatedChartWidth = data.length > visibleProjectCount
                ? Math.ceil(updatedViewportWidth * data.length / visibleProjectCount)
                : updatedViewportWidth;
            if (updatedChartWidth !== chartWidth) {
                chartWidth = updatedChartWidth;
                element.style.width = chartWidth + "px";
                chart.updateOptions({
                    chart: {
                        width: chartWidth
                    }
                });
            }
        }, 120);
    });
}

var overviewBreakdownCharts = {};

function bindDashboardListSearch() {
    var dashboard = document.querySelector(".dashboard-overview");
    if (!dashboard) {
        return;
    }

    dashboard.querySelectorAll("[data-dashboard-list-search]")
        .forEach(function (input) {
            var listBody = document.getElementById(
                input.getAttribute("data-dashboard-list-search"));
            if (!listBody) {
                return;
            }

            var rows = Array.prototype.slice.call(
                listBody.querySelectorAll("[data-search-row]"));
            var emptyRow = listBody.querySelector("[data-search-empty]");
            var modal = input.closest(".modal");
            var countElement = modal
                ? modal.querySelector("[data-breakdown-count]")
                : null;
            var projectSelect = modal
                ? modal.querySelector("[data-dashboard-list-project]")
                : null;

            function resetProjectFilter() {
                if (!projectSelect) {
                    return;
                }

                var activeCategory = modal.getAttribute("data-breakdown-active-category");
                var projects = Object.create(null);
                rows.forEach(function (row) {
                    var id = row.getAttribute("data-project-id");
                    if (id && (!activeCategory
                        || row.getAttribute("data-breakdown-category") === activeCategory)) {
                        projects[id] = row.getAttribute("data-project-label") || id;
                    }
                });
                projectSelect.options.length = 1;
                Object.keys(projects).sort(function (left, right) {
                    return projects[left].localeCompare(projects[right]);
                }).forEach(function (id) {
                    var option = document.createElement("option");
                    option.value = id;
                    option.textContent = projects[id];
                    projectSelect.appendChild(option);
                });
                projectSelect.value = "";
            }

            function applySearch() {
                var query = (input.value || "").trim().toLocaleLowerCase();
                var activeCategory = modal
                    ? modal.getAttribute("data-breakdown-active-category")
                    : "";
                var visibleCount = 0;

                rows.forEach(function (row) {
                    var matchesCategory = !activeCategory
                        || row.getAttribute("data-breakdown-category")
                            === activeCategory;
                    var matchesSearch = !query || row.textContent
                        .toLocaleLowerCase().indexOf(query) >= 0;
                    var matchesProject = !projectSelect || !projectSelect.value
                        || row.getAttribute("data-project-id") === projectSelect.value;
                    var matches = matchesCategory && matchesSearch && matchesProject;
                    row.classList.toggle("d-none", !matches);
                    if (matches) {
                        visibleCount += 1;
                    }
                });

                if (emptyRow) {
                    emptyRow.classList.toggle("d-none", visibleCount > 0);
                }

                if (countElement) {
                    countElement.textContent = visibleCount;
                }
            }

            input.addEventListener("input", applySearch);
            if (projectSelect) {
                projectSelect.addEventListener("change", applySearch);
            }

            if (modal) {
                modal.addEventListener("show.bs.modal", function () {
                    input.value = "";
                    resetProjectFilter();
                    applySearch();
                });
            }
        });
}

function formatOverviewBreakdownValue(value, data) {
    var formattedValue = Number(value || 0).toLocaleString(undefined, {
        maximumFractionDigits: 0
    });

    if (data && data.currencySuffix) {
        return formattedValue + data.currencySuffix;
    }

    return formattedValue;
}

function openOverviewBreakdownModal(type, data, dataPointIndex) {
    var key = data.keys && data.keys[dataPointIndex];
    var label = data.labels && data.labels[dataPointIndex];
    if (!key || !label || !window.bootstrap || !window.bootstrap.Modal) {
        return;
    }

    var modalId = type === "resource"
        ? "overviewResourceBreakdownModal"
        : "overviewCostBreakdownModal";
    var modal = document.getElementById(modalId);
    if (!modal) {
        return;
    }

    var title = modal.querySelector("[data-breakdown-title]");
    var search = modal.querySelector("[data-dashboard-list-search]");

    if (title) {
        title.textContent = label;
    }

    modal.setAttribute("data-breakdown-active-category", key);
    if (search) {
        search.value = "";
        var searchEvent = document.createEvent("HTMLEvents");
        searchEvent.initEvent("input", true, false);
        search.dispatchEvent(searchEvent);
    }

    window.bootstrap.Modal.getOrCreateInstance(modal).show();
}

function renderOverviewBreakdownChart(type) {
    var chartId = type === "resource"
        ? "overview-resource-breakdown-chart"
        : "overview-cost-breakdown-chart";
    var element = document.getElementById(chartId);
    var allData = window.dashboardOverviewBreakdownData || {};
    var data = allData[type];

    if (!element || !data) {
        return;
    }

    if (overviewBreakdownCharts[type]) {
        overviewBreakdownCharts[type].destroy();
        delete overviewBreakdownCharts[type];
    }
    element.innerHTML = "";

    var modal = document.getElementById(type === "resource"
        ? "overviewResourceBreakdownModal"
        : "overviewCostBreakdownModal");
    if (modal && !modal.__overviewChartResetBound) {
        modal.__overviewChartResetBound = true;
        modal.addEventListener("hidden.bs.modal", function () {
            modal.removeAttribute("data-breakdown-active-category");
            var projectSelect = modal.querySelector("[data-dashboard-list-project]");
            if (projectSelect) {
                projectSelect.value = "";
            }
            var search = modal.querySelector("[data-dashboard-list-search]");
            if (search) {
                search.value = "";
                var searchEvent = document.createEvent("HTMLEvents");
                searchEvent.initEvent("input", true, false);
                search.dispatchEvent(searchEvent);
            }
            renderOverviewBreakdownChart(type);
        });
    }

    if (typeof ApexCharts === "undefined") {
        element.textContent = data.noData || "";
        return;
    }

    var values = (data.values || []).map(function (value) {
        return Number(value || 0);
    });
    var total = values.reduce(function (sum, value) {
        return sum + value;
    }, 0);

    if (total <= 0) {
        element.innerHTML = '<div class="text-center text-muted py-5">'
            + escapeDashboardHtml(data.noData || "")
            + "</div>";
        return;
    }

    var palette = type === "resource"
        ? ["#94a3b8", "#f59e0b", "#22c55e", "#ef4444"]
        : ["#22c55e", "#f59e0b", "#ef4444"];
    var chart = new ApexCharts(element, {
        chart: {
            type: "donut",
            height: 300,
            toolbar: { show: false },
            events: {
                dataPointSelection: function (event, chartContext, config) {
                    openOverviewBreakdownModal(
                        type,
                        data,
                        config.dataPointIndex);
                }
            }
        },
        series: values,
        labels: data.labels || [],
        colors: palette,
        stroke: {
            colors: ["#fff"],
            width: 2
        },
        dataLabels: {
            enabled: true,
            formatter: function (percentage, options) {
                return formatOverviewBreakdownValue(
                    options.w.config.series[options.seriesIndex],
                    data);
            },
            style: {
                fontSize: "11px",
                fontWeight: 600
            },
            dropShadow: { enabled: false }
        },
        plotOptions: {
            pie: {
                expandOnClick: false,
                donut: {
                    size: "66%",
                    labels: {
                        show: true,
                        name: {
                            show: false
                        },
                        value: {
                            show: false
                        },
                        total: {
                            show: true,
                            label: data.centerLabel || "",
                            formatter: function () {
                                return formatOverviewBreakdownValue(total, data);
                            }
                        }
                    }
                }
            }
        },
        legend: {
            position: "bottom",
            fontSize: "12px",
            formatter: function (seriesName, options) {
                var value = options.w.globals.series[options.seriesIndex];
                return escapeDashboardHtml(seriesName) + " — "
                    + formatOverviewBreakdownValue(value, data);
            },
            onItemClick: { toggleDataSeries: false },
            onItemHover: { highlightDataSeries: true }
        },
        tooltip: {
            y: {
                formatter: function (value) {
                    return formatOverviewBreakdownValue(value, data);
                }
            }
        },
        states: {
            hover: { filter: { type: "lighten", value: 0.04 } },
            active: { filter: { type: "none" } }
        },
        responsive: [{
            breakpoint: 576,
            options: {
                chart: { height: 280 },
                legend: { fontSize: "11px" },
                dataLabels: { enabled: false }
            }
        }]
    });

    overviewBreakdownCharts[type] = chart;
    chart.render();
    element.style.cursor = "pointer";
}

document.addEventListener("DOMContentLoaded", function () {
    bindDashboardListSearch();
    renderProjectStatusChart();
    renderProjectProgressChart();
    renderOverviewBreakdownChart("resource");
    renderOverviewBreakdownChart("cost");
});
