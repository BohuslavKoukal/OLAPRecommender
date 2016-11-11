$(document).ready(function () {
    $(function () {
        Highcharts.setOptions({
            chart: {
                borderWidth: 5,
                borderColor: '#e8eaeb',
                borderRadius: 0,
                backgroundColor: '#f7f7f7'
            },
            title: {
                style: {
                    'fontSize': '1em'
                },
                useHTML: true,
                x: -27,
                y: 8,
                text: '<span class="chart-title">' + groupedChartTitle + '</span>'
            },
            xAxis: {
                type: 'category'
            },
            yAxis: {
                title: {
                    text: measureName
                }
            },
            plotOptions: {
                series: {
                    borderWidth: 0,
                    dataLabels: {
                        enabled: true
                    }
                }
            }
        });
    });

    $(function () {
        // example how data could look like
        var series = [
            {
                name: "Asia",
                data: [14, 44, 18, 15, 16, 25, 2]
            }, {
                name: "Europe",
                data: [24, 34, 8, 25, 0, 35, 4]
            }
        ];
        var categories = [
            {
                name: "Bakery",
                categories: ["Bread", "Bun", "Cakes"]
            }, {
                name: "Dairy",
                categories: ["Butter", "Milk", "Cheese"]
            }, {
                name: "Electronics",
                categories: ["Notebook"]
            }
        ];


        window.chart = new Highcharts.Chart({
            chart: {
                renderTo: "grouped-chart",
                type: "column"
            },
            series: groupedSerializedSeries,
            xAxis: {
                categories: groupedSerializedCategories
            }
        });
    });



});