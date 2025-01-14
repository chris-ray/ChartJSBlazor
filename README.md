# ChartJs interop with Blazor

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/Joelius300/ChartJSBlazor/blob/master/LICENSE.md)
[![GitHub issues](https://img.shields.io/github/issues/Joelius300/ChartJSBlazor.svg)](https://github.com/Joelius300/ChartJSBlazor/issues)
[![GitHub forks](https://img.shields.io/github/forks/Joelius300/ChartJSBlazor.svg)](https://github.com/Joelius300/ChartJSBlazor/network)
[![GitHub stars](https://img.shields.io/github/stars/Joelius300/ChartJSBlazor.svg)](https://github.com/Joelius300/ChartJSBlazor/stargazers)
[![Join the chat at https://gitter.im/ChartJSBlazor/community](https://badges.gitter.im/ChartJSBlazor/community.svg)](https://gitter.im/ChartJSBlazor/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

This is a Blazor Component that wraps [ChartJS](https://github.com/chartjs/Chart.js).
You can use the library in both client- and server-side projects.

## Introduction

This library is a modification of [this awesome library](https://github.com/mariusmuntean/ChartJs.Blazor). 
I intend to extend the functionality (mainly of the LineChart), fix issues and add more options from Chart.js (completeness).  

~~I fully intend giving all of this back to the community so that the original repository also gets all of these changes. Sadly because it's not a simple fork I don't know how to do that directly (let me know if you can help me with that :).~~  
Since it has now become apparent that the old repo is not maintained anymore, I'd be happy to continue this project here. Since this is a project I'm working on in my free time, don't expect this to grow rapidly.  There will often be differences between the docs and the actual code so I really advise you to go look at the [WebCore-project](https://github.com/Joelius300/ChartJSBlazor/tree/master/WebCore) which contains several examples that are up-to-date.

## Changelog

### Latest changes
**0.10.2:**
    
* Update ReadMe
* Clean and update .csproj file
* Create nuget package
* Update XML-docs handling

The full changelog can be found [here](https://github.com/Joelius300/ChartJSBlazor/blob/master/CHANGELOG.md).

## Please keep in mind that this is still a preview. Expect breaking changes during the next releases. We're reworking all the charts because most of them contain errors and inconsistencies.

## Prerequisites

Don't know what Blazor is? Read [here](https://dotnet.microsoft.com/apps/aspnet/web-apps/client).

The prerequisites are:

1. Visual Studio 2019 preview 2
2. .Net core 3 preview7


## Installation

There's a NuGet package: https://www.nuget.org/packages/ChartJs.Blazor.Fork/

Install from the command line:

```bash
dotnet add package ChartJs.Blazor.Fork
```

## Usage

For detailed instruction go to the [Wiki page](https://github.com/Joelius300/ChartJSBlazor/wiki). Since the example here and those in the wiki might be outdated very fast because of the many breaking changes, I would also advise you to go look at the WebCore project in this repos where you can find some examples.  

Before you can start creating a chart with a config etc, you have to include some static assets to your project.

In your `_Host.cshtml` (server-side) or in your `index.html` (client-side) -file add this code to have `moment.js` with the locales:

```html
<script src="https://cdnjs.cloudflare.com/ajax/libs/moment.js/2.24.0/moment-with-locales.min.js"
        integrity="sha256-AdQN98MVZs44Eq2yTwtoKufhnU+uZ7v2kXnD5vqzZVo="
        crossorigin="anonymous">
</script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/Chart.js/2.8.0/Chart.min.js"></script>
```

or this code if you want the bundled version of `Chart.Js`, but without the locales of `moment.js` (`moment.js` itself is then included in the bundle):

```html
<script src="https://cdnjs.cloudflare.com/ajax/libs/Chart.js/2.8.0/Chart.bundle.min.js"></script> <!--Contains moment.js for time axis-->
```

Furthermore, you need to include the js-interop and the css-file which enables responsiveness.  
Since those are static assets in the library, you should be able to reference them via your `_Host.cshtml`/`index.html`-file directly, without copying the files. However there a few catches.  
First of all, it doesn't seem to work on server-side yet even though they've removed the mention in [the docs](https://docs.microsoft.com/de-de/aspnet/core/razor-pages/ui-class?view=aspnetcore-3.0&tabs=visual-studio). I'm not sure what the issue is but in case it doesn't work just manually grab them from [here](https://github.com/Joelius300/ChartJSBlazor/tree/master/ChartJs.Blazor/wwwroot) and reference them directly in your project.  
Otherwise the code to reference them would be the following.
```html
<script src="_content/ChartJs.Blazor/ChartJsInterop.js" type="text/javascript" language="javascript"></script>
<link rel="stylesheet" href="_content/ChartJs.Blazor/ChartJsBlazor.css" />
```

Now to creating the chart.  
Below is a simple example for a line-chart. You can find this also [here](https://github.com/Joelius300/ChartJSBlazor/blob/master/WebCore/Pages/SimpleLineLinearExample.razor) (the link is probably more up to date in  case the below code doesn't work).  

The example covers a few static options, how to use a simple point-dataset and how to dynamically initialize and update the data and the chart.  

```csharp
@page "/SimpleLineLinearExample"
@using WebCore.Data
@using ChartJs.Blazor.Charts
@using ChartJs.Blazor.ChartJS.Common
@using ChartJs.Blazor.ChartJS.Common.Properties
@using ChartJs.Blazor.ChartJS.Common.Enums
@using ChartJs.Blazor.ChartJS.Common.Legends
@using ChartJs.Blazor.ChartJS.LineChart
@using ChartJs.Blazor.ChartJS.LineChart.Axes
@using ChartJs.Blazor.ChartJS.LineChart.Axes.Ticks
@using ChartJs.Blazor.Util.Color

<h2>Line Linear Chart</h2>
<ChartJsLineChart @ref="lineChartJs" Config="@lineChartConfig" Width="600" Height="300" />
<Button @onclick="@UpdateChart">Add random point</Button>

@functions
{
    LineChartConfig lineChartConfig;
    ChartJsLineChart lineChartJs;

    private LineChartDataset<Point> pointDataset;

    private Random rnd = new Random();

    protected override void OnInit()
    {
        lineChartConfig = new LineChartConfig
        {
            CanvasId = "mySimpleLineChart",
            Options = new LineChartOptions
            {
                Responsive = true,
                Title = new OptionsTitle
                {
                    Display = true,
                    Text = "Simple Line Chart"
                },
                Legend = new Legend
                {
                    Position = Positions.Right,
                    Labels = new LegendLabelConfiguration
                    {
                        UsePointStyle = true
                    }
                },
                Tooltips = new Tooltips
                {
                    Mode = InteractionMode.Nearest,
                    Intersect = false
                },
                Scales = new Scales
                {
                    xAxes = new List<CartesianAxis>
                    {
                        new LinearCartesianAxis
                        {
                            ScaleLabel = new ScaleLabel
                            {
                                LabelString = "X-value"
                            }
                        }
                    },
                    yAxes = new List<CartesianAxis>()
                    {
                        new LinearCartesianAxis
                        {
                            ScaleLabel = new ScaleLabel
                            {
                                LabelString = "Random value"
                            }
                        }
                    }
                }
            }
        };


        pointDataset = new LineChartDataset<Point>()
        {
            BackgroundColor = ColorUtil.ColorString(0, 255, 0, 1.0),
            BorderColor = ColorUtil.ColorString(0, 0, 255, 1.0),
            Label = "Some values",
            Fill = false,
            PointBackgroundColor = ColorUtil.RandomColorString(),
            BorderWidth = 1,
            PointRadius = 3,
            PointBorderWidth = 1
        };

        pointDataset.AddRange(Enumerable.Range(1, 10).Select(i => new Point(i, rnd.Next(30))));

        lineChartConfig.Data.Datasets.Add(pointDataset);
    }

    private void UpdateChart()
    {
        pointDataset.Add(new Point(pointDataset.Data.Last().X +1, rnd.Next(rnd.Next(50))));
        lineChartJs.Update();
    }
}
```

# Contributors
* [Joelius300](https://github.com/Joelius300)
* [SeppPenner](https://github.com/SeppPenner)
* [MindSwipe](https://github.com/MindSwipe)

# Contributing
We really like people helping us with the project. Nevertheless, take your time to read our contributing guidelines [here](https://github.com/Joelius300/ChartJSBlazor/blob/master/CONTRIBUTING.md).
