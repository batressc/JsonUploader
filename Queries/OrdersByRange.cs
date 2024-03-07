using JsonUploader.Models;
using System.Globalization;
using System.Text.Json;

namespace JsonUploader.Queries;

internal class OrdersByRange {
    private readonly List<JsonInformation> _data;

    public OrdersByRange(List<JsonInformation> data) {
        _data = data;
    }

    public List<JsonInformation> ExecuteQuery(DateTime startDate, DateTime endDate) {
        List<JsonInformation> result = _data.Where(dat => {
            JsonElement jsonRecord = dat.JsonObject.RootElement;
            if (jsonRecord.ValueKind != JsonValueKind.Object) {
                return false;
            }
            JsonElement requestedCompletionDate = jsonRecord.GetProperty("requestedCompletionDate");
            if (requestedCompletionDate.ValueKind != JsonValueKind.String) {
                return false;
            }
            DateTime.TryParseExact(
                requestedCompletionDate.GetString(),
                "yyyy-MM-ddTHH:mm:ss.fffzzz",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime datetimeValue
            );
            return datetimeValue >= startDate && datetimeValue <= endDate;
        }).ToList();
        return result;
    }
}
