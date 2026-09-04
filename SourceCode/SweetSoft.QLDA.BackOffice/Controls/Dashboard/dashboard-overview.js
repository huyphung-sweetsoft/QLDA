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
        console.error("ApexCharts chưa được load.");
        return;
    }

    if (!window.projectStatusChartData) {
        console.error("projectStatusChartData chưa tồn tại.");
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
        console.error("ApexCharts chưa được load.");
        return;
    }

    if (!window.projectProgressChartData) {
        console.error("projectProgressChartData chưa tồn tại.");
        return;
    }

    var data = window.projectProgressChartData;

    var chartHeight = Math.max(360, data.length * 55);

    element.style.height = chartHeight + "px";

    var options = {

        chart: {
            type: "bar",
            height: chartHeight,
            toolbar: {
                show: false
            },
            parentHeightOffset: 0
        },
        grid: {
            padding: {
                top: 0,
                right: 10,
                bottom: 10,
                left: 10
            }
        },

        series: [
            {
                name: "Tiến độ",
                data: data.map(function (item) {
                    return Number(item.progress);
                })
            }
        ],

        xaxis: {
            categories: data.map(function (item) {
                return item.code;
            })
        },

        yaxis: {
            min: 0,
            max: 100,
            tickAmount: 5,
            title: {
                text: "Tiến độ (%)"
            }
        },

        plotOptions: {
            bar: {
                horizontal: true,
                barHeight: "50%",
                borderRadius: 4
            }
        },

        dataLabels: {
            enabled: true,
            textAnchor: "start",
            offsetX: 0,
            formatter: function (val) {
                return val + "%";
            },
            style: {
                colors: ["#fff"]
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
                    ${escapeDashboardHtml(item.name)}
                </div>
                <div>
                    <strong>Tiến độ:</strong>
                    ${item.progress}%
                </div>
                <div>
                    <strong>Bắt đầu:</strong>
                    ${escapeDashboardHtml(item.startDate)}
                </div>
                <div>
                    <strong>Dự kiến:</strong>
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
        }
    };
    chart.updateOptions(options);
}

document.addEventListener("DOMContentLoaded", function () {
    renderProjectStatusChart();
    renderProjectProgressChart();
});
