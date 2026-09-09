using System.Text.Json.Serialization;

namespace Ecommerce.Models
{
    public class CommonModel
    {

        public class CommonInputArray
        {
            [JsonPropertyName("ID_Value")]
            public int ID { get; set; }

        }

        public class TableOutput<U>
        {
            public List<U>? TableData { get; set; }
            public TableOutput_Settings? TableSettings { get; set; }
        }
        public class TableOutput_Settings
        {
            public long PageSize { get; set; }
            public long PageIndex { get; set; }
            public long TotalCount { get; set; }
        }
        public class CommonResponse
        {
            public long ResponseCode { get; set; }
            public bool StatusCode { get; set; }
            public string ResponseMsg { get; set; } = string.Empty;
        }

        public class MultipleTableOutput<T>
        {
            /// <summary>
            /// Data for the first table.
            /// </summary>
            public TableMultipleOutput<T>? TableData1 { get; set; }
            public TableMultipleOutput<T>? TableData2 { get; set; }
            public TableMultipleOutput<T>? TableData3 { get; set; }
            public TableMultipleOutput<T>? TableData4 { get; set; }
            public TableMultipleOutput<T>? TableData5 { get; set; }
            public TableMultipleOutput<T>? TableData6 { get; set; }
            public TableMultipleOutput<T>? TableData7 { get; set; }
            public TableMultipleOutput<T>? TableData8 { get; set; }
        }
        public class TableMultipleOutput<T>
        {
            /// <summary>
            /// List of records representing table data.
            /// </summary>
            public List<T>? TableData { get; set; }
        }


        public class TableKeyMapping
        {
            public string ColumnKey { get; set; } = string.Empty;
            public string ColumnValue { get; set; } = string.Empty;

        }
        public class CommonResponse<T>
        {
            public int StatusCode { get; set; }
            public string? Message { get; set; }
            public T? Data { get; set; }
            public object? Error { get; set; }
        }
        public class CommonError
        {
            public string? SerializerSettings { get; set; }
            public string? StatusCode { get; set; }
            public string? ContentType { get; set; }
            public object? Value { get; set; }
        }
        public class MultipleOutput<T1, T2>
        {
            public List<T1>? TableOut1 { get; set; }
            public List<T2>? TableOut2 { get; set; }
        }

        public class MultipleOutput<T1, T2, T3>
        {
            public List<T1>? TableOut1 { get; set; }
            public List<T2>? TableOut2 { get; set; }
            public List<T3>? TableOut3 { get; set; }
        }

        public class MultipleOutput<T1, T2, T3, T4>
        {
            public List<T1>? TableOut1 { get; set; }
            public List<T2>? TableOut2 { get; set; }
            public List<T3>? TableOut3 { get; set; }
            public List<T4>? TableOut4 { get; set; }
        }

        public class InfoList<TInfo, TData>
        {
            public TInfo? ListInfo { get; set; }
            public List<TData>? ListData { get; set; }
        }

        public class ApiResponse<T>
        {
            public bool Success { get; set; }

            public string Message { get; set; } = string.Empty;

            public T? Data { get; set; }

            public List<string> Errors { get; set; } = new();
        }
    }
}
