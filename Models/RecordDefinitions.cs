using System.Text.Json;

namespace JsonUploader.Models;

internal record JsonInformation(string FileName, string Content, JsonDocument JsonObject, Order? jsonOrder);
internal record ComponentRemovedDto(string FileName, string Content, IEnumerable<string> ProductsRemoved);

