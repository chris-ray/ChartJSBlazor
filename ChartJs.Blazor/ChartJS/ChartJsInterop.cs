﻿using Microsoft.JSInterop;
using System.Threading.Tasks;
using ChartJs.Blazor.ChartJS.Common;
using System;
using System.Dynamic;
using ChartJs.Blazor.ChartJS.Common.Legends.OnClickHandler;
using ChartJs.Blazor.ChartJS.Common.Legends.OnHover;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace ChartJs.Blazor.ChartJS
{
    public static class ChartJsInterop
    {
        public static async Task<bool> SetupChart(this IJSRuntime jsRuntime, ChartConfigBase chartConfig)
        {
            try
            {
                Console.WriteLine();
                Console.WriteLine("Direct: ");
                Console.WriteLine(await jsRuntime.GetJsonRep(chartConfig));

                dynamic dynParam = StripNulls(chartConfig);
                Dictionary<string, object> param = ConvertExpandoObjectToDictionary(dynParam);

                Console.WriteLine();
                Console.WriteLine("Parsed to Expando and then dict: ");
                Console.WriteLine(await jsRuntime.GetJsonRep(param));

                return await jsRuntime.InvokeAsync<bool>("ChartJSInterop.SetupChart", param);
            }
            catch (Exception exp)
            {
                Console.WriteLine($"Error while setting up chart: {exp.Message}");
            }

            //return Task.FromResult<bool>(false);
            return false;
        }

        private static Task<string> GetJsonRep(this IJSRuntime jSRuntime, object obj) => jSRuntime.InvokeAsync<string>("getStringRep", obj);

        /// <summary>
        /// This method is specifically used to convert an <see cref="ExpandoObject"/> with a Tree structure to a <see cref="Dictionary{string, object}"/>.
        /// </summary>
        /// <param name="expando">The <see cref="ExpandoObject"/> to convert</param>
        /// <returns>The fully converted <see cref="ExpandoObject"/></returns>
        private static Dictionary<string, object> ConvertExpandoObjectToDictionary(ExpandoObject expando) => RecursivelyConvertIDictToDict(expando);

        /// <summary>
        /// This method takes an <see cref="IDictionary{string, object}"/> and recursively converts it to a <see cref="Dictionary{string, object}"/>. 
        /// The idea is that every <see cref="IDictionary{string, object}"/> in the tree will be of type <see cref="Dictionary{string, object}"/> instead of some other implementation like <see cref="ExpandoObject"/>.
        /// </summary>
        /// <param name="value">The <see cref="IDictionary{string, object}"/> to convert</param>
        /// <returns>The fully converted <see cref="Dictionary{string, object}"/></returns>
        private static Dictionary<string, object> RecursivelyConvertIDictToDict(IDictionary<string, object> value) =>
            value.ToDictionary(
                keySelector => keySelector.Key,
                elementSelector =>
                {
                    // if it's another IDict just go through it recursively
                    if (elementSelector.Value is IDictionary<string, object> dict)
                    {
                        return RecursivelyConvertIDictToDict(dict);
                    }

                    // if it's an IEnumerable check each element
                    if (elementSelector.Value is IEnumerable<object> list)
                    {
                        // go through all objects in the list
                        // if the object is an IDict -> convert it
                        // if not keep it as is
                        return list
                            .Select(o => o is IDictionary<string, object>
                                ? RecursivelyConvertIDictToDict((IDictionary<string, object>)o)
                                : o
                            );
                    }

                    // neither an IDict nor an IEnumerable -> it's fine to just return the value it has
                    return elementSelector.Value;
                }
            );

        public static Task<bool> UpdateChart(this IJSRuntime jsRuntime, ChartConfigBase chartConfig)
        {
            try
            {
                //dynamic dynParam = StripNulls(chartConfig);
                //Dictionary<string, object> param = ConvertExpandoObjectToDictionary(dynParam);
                return jsRuntime.InvokeAsync<bool>("ChartJSInterop.UpdateChart", chartConfig);
            }
            catch (Exception exp)
            {
                Console.WriteLine($"Error while updating chart: {exp.Message}");
            }

            return Task.FromResult<bool>(false);
        }

        /// <summary>
        /// Returns an object that is equivalent to the given parameter but without any null member AND it preserves DotNetInstanceClickHandler/DotNetInstanceHoverHandler members intact
        ///
        /// <para>Preserving DotNetInstanceClick/HoverHandler members is important because they contain DotNetObjectRefs to the instance whose method should be invoked on click/hover</para>
        ///
        /// <para>This whole method is hacky af but necessary. Stripping null members is only needed because the default config for the Line charts on the Blazor side is somehow messed up. If this were not the case no null member stripping were necessary and hence, the recovery of the DotNetObjectRef members would also not be needed. Nevertheless, The Show must go on!</para>
        /// </summary>
        /// <param name="chartConfig"></param>
        /// <returns></returns>
        private static ExpandoObject StripNulls(object chartConfig)
        {
            // Serializing with the custom serializer settings remove null members
            var cleanChartConfigStr = JsonConvert.SerializeObject(chartConfig, JsonSerializerSettings);

            Console.WriteLine();
            Console.WriteLine("The clean json string serialized from json.net");
            Console.WriteLine(cleanChartConfigStr);

            // Get back an ExpandoObject dynamic with the clean config - having an ExpandoObject allows us to add/replace members regardless of type
            dynamic clearConfigExpando = JsonConvert.DeserializeObject<ExpandoObject>(cleanChartConfigStr, new ExpandoObjectConverter());
            return clearConfigExpando;
            // Restore any .net refs that need to be passed intact
            var dynamicChartConfig = (dynamic) chartConfig;
            if (dynamicChartConfig?.Options?.Legend?.OnClick != null
                && dynamicChartConfig?.Options?.Legend?.OnClick is DotNetInstanceClickHandler)
            {
                clearConfigExpando.options = clearConfigExpando.options ?? new { };
                clearConfigExpando.options.legend = clearConfigExpando.options.legend ?? new { };
                clearConfigExpando.options.legend.onClick = dynamicChartConfig.Options.Legend.OnClick;
            }

            if (dynamicChartConfig?.Options?.Legend?.OnHover != null
                && dynamicChartConfig?.Options?.Legend?.OnHover is DotNetInstanceHoverHandler)
            {
                clearConfigExpando.options = clearConfigExpando.options ?? new { };
                clearConfigExpando.options.legend = clearConfigExpando.options.legend ?? new { };
                clearConfigExpando.options.legend.onHover = dynamicChartConfig.Options.Legend.OnHover;
            }

            return clearConfigExpando;
        }

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy(true, false)
            }
        };


        public static async Task DemoSOThing(this IJSRuntime jsRuntime)
        {
            SomeConfig config = new SomeConfig
            {
                Options = new SomeOptions   // it can contain complex types
                {
                    SomeInt = 2,            // it can contain primative types
                    SomeString = null,
                    Axes = new List<Axis>   // it can contain complex lists
                    {
                        new Axis(),
                        new Axis
                        {
                            SomeString = "axisString"
                        }
                    }
                },
                Data = new SomeData
                {
                    Data = new List<int> { 1, 2, 3, 4, 5 },     // it can contain primative lists
                    SomeString = "asdf",
                    SomeStringEnum = MyStringEnum.Test          // it can contain objects with custom parsing (for JSON.NET, not the parsing that's done when invoking the JS sadly
                }
            };

            Console.WriteLine();
            Console.WriteLine("Direct: ");
            Console.WriteLine(await jsRuntime.GetJsonRep(config));

            dynamic dynParam = StripNulls(config);
            Dictionary<string, object> param = ConvertExpandoObjectToDictionary(dynParam);

            Console.WriteLine();
            Console.WriteLine("Parsed to Expando and then dict: ");
            Console.WriteLine(await jsRuntime.GetJsonRep(param));
        }

        public class SomeConfig
        {
            public SomeOptions Options { get; set; }
            public SomeData Data { get; set; }
        }

        public class SomeOptions
        {
            public int SomeInt { get; set; }

            public string SomeString { get; set; }

            public List<Axis> Axes { get; set; }
        }

        public class SomeData
        {
            public List<int> Data { get; set; }

            public string SomeString { get; set; }

            public MyStringEnum SomeStringEnum { get; set; }
        }

        public class Axis
        {
            public string SomeString { get; set; }
        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public class MyStringEnum
        {
            public static MyStringEnum Test => new MyStringEnum("someTestThing");


            private readonly string _value;
            private MyStringEnum(string stringRep) => _value = stringRep;
            public override string ToString() => _value;
        }

        public class JsonStringEnumConverter : JsonConverter<MyStringEnum>
        {
            public sealed override bool CanRead => false;
            public sealed override bool CanWrite => true;

            public sealed override MyStringEnum ReadJson(JsonReader reader, Type objectType, MyStringEnum existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException("Don't use me to read JSON");
            }

            public override void WriteJson(JsonWriter writer, MyStringEnum value, JsonSerializer serializer)
            {
                // ToString was overwritten by StringEnum -> safe to just print the string representation
                writer.WriteValue(value.ToString());
            }
        }
    }
}