﻿using InnerTube;
using InnerTube.Protobuf;
using MongoDB.Bson.Serialization.Attributes;

namespace LightTube.Database.Models;

[BsonIgnoreExtraElements]
public class DatabaseVideoAuthor
{
    public string Id;
    public string Name;
}