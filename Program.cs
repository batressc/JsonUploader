using JsonUploader.Models;
using JsonUploader.Queries;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonUploader;

class Program {
    private static async Task<List<JsonInformation>?> GetData() {
        string currentPath = Path.Combine(Directory.GetCurrentDirectory(), "JsonData");
        if (!Directory.Exists(currentPath)) {
            Console.WriteLine("You must put json files to read on JsonData directory.");
            return null;
        }
        string[] jsonFiles = Directory.GetFiles(currentPath, "*.json");
        List<JsonInformation> data = [];
        foreach (string file in jsonFiles) {
            string content = await File.ReadAllTextAsync(file);
            data.Add(
                new JsonInformation(
                    Path.GetFileNameWithoutExtension(file),
                    content,
                    JsonDocument.Parse(content)
                )
            );
        }
        return data;
    }

    static async Task Main(string[] args) {
        List<JsonInformation>? jsonData = await GetData();
        if (jsonData is null) {
            return;
        }
        OrdersByRange ordersByRange = new(jsonData);
        ordersByRange.ExecuteQuery(new(2019, 1, 1), new(2024, 1, 1));

        ComponentsRemoved componentsRemoved = new(jsonData);
        componentsRemoved.ExecuteQuery();

        DryLoopRemoved dryLoopRemoved = new(jsonData);
        dryLoopRemoved.ExecuteQuery();

        OneAdditionalSetupBox oneAdditionalSetupBox = new(jsonData);
        oneAdditionalSetupBox.ExecuteQuery();

        OnlyMainSetupBox onlyMainSetupBox = new(jsonData);
        onlyMainSetupBox.ExecuteQuery();

        WithoutPromotions withoutPromotions = new(jsonData);
        withoutPromotions.ExecuteQuery();
    }
}
