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

        if (!element || typeof ApexCharts === "undefined") {
            return;
        }

        if (data.length === 0) {
            showEmptyState(element, texts.noCompletedProjectComparison || "", 320);
            return;
        }

        var chartHeight = Math.max(340, data.length * 72);
        element.style.height = chartHeight + "px";
        element.style.cursor = data.some(function (item) {
            return !!item.detailUrl;
        }) ? "pointer" : "";

        new ApexCharts(element, {
            chart: {
                type: "bar",
                height: chartHeight,
                toolbar: { show: false },
                parentHeightOffset: 0,
                events: {
                    dataPointSelection: function (event, chartContext, config) {
                        var item = data[config.dataPointIndex];

                        if (item && item.detailUrl) {
                            window.location.assign(item.detailUrl);
                        }
                    }
                }
            },
            series: [
                {
                    name: texts.contractValue || "",
                    data: data.map(function (item) {
                        return Number(item.contractValue) || 0;
                    })
                },
                {
                    name: texts.actualCost || "",
                    data: data.map(function (item) {
                        return Number(item.actualCost) || 0;
                    })
                }
            ],
            colors: ["#556ee6", "#f46a6a"],
            plotOptions: {
                bar: {
                    horizontal: true,
                    barHeight: "62%",
                    borderRadius: 3
                }
            },
            xaxis: {
                min: 0,
                categories: data.map(function (item) {
                    return item.code;
                }),
                labels: {
                    formatter: function (value) {
                        return formatMoney(value);
                    }
                }
            },
            dataLabels: { enabled: false },
            legend: { position: "top", horizontalAlign: "right" },
            tooltip: {
                shared: true,
                intersect: false,
                x: {
                    formatter: function (value, options) {
                        var item = data[options.dataPointIndex];
                        return item ? item.code + " - " + item.name : value;
                    }
                },
                y: {
                    formatter: function (value) {
                        return formatMoney(value);
                    }
                }
            }
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
        var total = received + outstanding;

        if (!element || typeof ApexCharts === "undefined") {
            return;
        }

        if (total === 0) {
            showEmptyState(element, texts.noContractOrPayment || "", 300);
            return;
        }

        new ApexCharts(element, {
            chart: {
                type: "donut",
                height: 310,
                toolbar: { show: false }
            },
            labels: [texts.received || "", texts.outstanding || ""],
            series: [received, outstanding],
            colors: ["#34c38f", "#f1b44c"],
            legend: { position: "bottom" },
            dataLabels: { enabled: true },
            tooltip: {
                y: {
                    formatter: function (value) {
                        return formatMoney(value);
                    }
                }
            },
            plotOptions: {
                pie: {
                    donut: {
                        size: "68%",
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
            }
        }).render();
    }

    function renderCostTrendChart() {
        var element = document.getElementById("cost-trend-chart");
        var data = window.dashboardCostTrendData || [];

        if (!element || typeof ApexCharts === "undefined") {
            return;
        }

        if (data.length === 0) {
            showEmptyState(element, texts.noCostTrend || "", 300);
            return;
        }

        new ApexCharts(element, {
            chart: {
                type: "area",
                height: 330,
                toolbar: { show: false },
                zoom: { enabled: false }
            },
            series: [{
                name: texts.incurredCost || "",
                data: data.map(function (item) {
                    return Number(item.amount) || 0;
                })
            }],
            colors: ["#f46a6a"],
            stroke: {
                curve: "smooth",
                width: 3
            },
            fill: {
                type: "gradient",
                gradient: {
                    shadeIntensity: 1,
                    opacityFrom: 0.35,
                    opacityTo: 0.05,
                    stops: [0, 95, 100]
                }
            },
            xaxis: {
                categories: data.map(function (item) {
                    return item.month;
                })
            },
            yaxis: {
                labels: {
                    formatter: function (value) {
                        return formatMoney(value);
                    }
                }
            },
            dataLabels: { enabled: false },
            tooltip: {
                y: {
                    formatter: function (value) {
                        return formatMoney(value);
                    }
                }
            }
        }).render();
    }

    document.addEventListener("DOMContentLoaded", function () {
        renderProjectComparisonChart();
        renderPaymentChart();
        renderCostTrendChart();
    });
})();
