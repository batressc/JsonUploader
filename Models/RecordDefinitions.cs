using System.Text.Json;

namespace JsonUploader.Models;

internal record JsonInformation(string FileName, string Content, JsonDocument JsonObject, Order? Order);
internal record ComponentRemovedDto(string FileName, string Content, IEnumerable<string> ProductsRemoved);

