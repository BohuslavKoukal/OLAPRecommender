$(document).ready(function () {

    $(function () {
        // Create the chart
        $('#container').highcharts({
            chart: {
                type: 'column'
            },
            title: {
                text: drilldownChartTitle
            },
            subtitle: {
                text: ''
            },
            xAxis: {
                type: 'category'
            },
            yAxis: {
                title: {
                    text: measureName
                }

            },
            legend: {
                enabled: false
            },
            plotOptions: {
                series: {
                    borderWidth: 0,
                    dataLabels: {
                        enabled: true,
                        format: '{point.y}'
                    }
                }
            },

            tooltip: {
                headerFormat: '<span style="font-size:11px">{series.name}</span><br>',
                pointFormat: '<span style="color:{point.color}">{point.name}</span>: <b>{point.y}</b><br/>'
            },
            
            series:
                drilldownSerializedSeries,
            drilldown:
                drilldownSerializedDrilldown
        });
    });

   

});