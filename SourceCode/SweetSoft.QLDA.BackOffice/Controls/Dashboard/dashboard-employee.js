document.addEventListener("DOMContentLoaded", function () {
    renderEmployeeProjectChart();
});

function renderEmployeeProjectChart() {
    var element = document.getElementById("employee-project-chart");

    if (!element) return;
    if (typeof ApexCharts === "undefined") return;
    if (!window.employeeProjectChartData || window.employeeProjectChartData.length === 0) return;

    var data = window.employeeProjectChartData;
    var chartHeight = Math.max(300, data.length * 55);
    element.style.height = chartHeight + "px";

    var chart = new ApexCharts(element, {
        chart: {
            type: "bar",
            height: chartHeight,
            toolbar: { show: false },
            parentHeightOffset: 0
        },
        grid: {
            padding: { top: 0, right: 10, bottom: 10, left: 10 }
        },
        series: [{
            name: "Tiến độ",
            data: data.map(function (item) { return Number(item.progress); })
        }],
        xaxis: {
            categories: data.map(function (item) { return item.code; })
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
            formatter: function (val) { return val + "%"; },
            style: { colors: ["#fff"] }
        },
        tooltip: {
            intersect: false,
            followCursor: true,
            y: {
                formatter: function (val) { return val + "%"; }
            }
        },
        legend: { show: false }
    });

    chart.render();
}
