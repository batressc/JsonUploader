using JsonUploader.Models;
using JsonUploader.Queries;
using System.Text.Json;

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
        JsonSerializerOptions jsonSerializerOptions = new() {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        foreach (string file in jsonFiles) {
            string content = await File.ReadAllTextAsync(file);
            data.Add(
                new JsonInformation(
                    Path.GetFileNameWithoutExtension(file),
                    content,
                    JsonDocument.Parse(content),
                    JsonSerializer.Deserialize<Order>(content, jsonSerializerOptions)
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
        QueriesLinQ queriesLinQ = new(jsonData);

        // 1. Obtener todas las órdenes filtrando por rango de fechas
        OrdersByRange ordersByRange = new(jsonData);
        ordersByRange.ExecuteQuery(new(2023, 4, 5), new(2023, 4, 6));
        queriesLinQ.OrdersByRange(new(2023, 4, 5), new(2023, 4, 6));

        // 2. Obtener todos los componentes removidos por orden
        ComponentsRemoved componentsRemoved = new(jsonData);
        componentsRemoved.ExecuteQuery();
        queriesLinQ.ComponentsRemoved();

        // 6. Obtener todas las órdenes que remuevan dry loop
        DryLoopRemoved dryLoopRemoved = new(jsonData);
        dryLoopRemoved.ExecuteQuery();
        queriesLinQ.DryLoopRemoved();

        // 7. Obtener todas las órdenes que tengan solo un additional STB
        OneAdditionalSetupBox oneAdditionalSetupBox = new(jsonData);
        oneAdditionalSetupBox.ExecuteQuery();
        queriesLinQ.OneAdditionalSetupBox();

        // 8. Obtener todas las órdenes que tengan no tengan additional STB pero tengan STB principal
        OnlyMainSetupBox onlyMainSetupBox = new(jsonData);
        onlyMainSetupBox.ExecuteQuery();
        queriesLinQ.OnlyMainSetupBox();

        // 9. Obtener las órdenes que no tengan promociones
        WithoutPromotions withoutPromotions = new(jsonData);
        withoutPromotions.ExecuteQuery();
        queriesLinQ.WithoutPromotions();
    }
}
