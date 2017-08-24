# MongoDB.Repository
### Render
```C#
var filter = UserRepAsync.Filter.Eq(x => x.Name, "aa") & UserRepAsync.Filter.Eq(x => x.ID, 123);
BsonDocument filterBson = userRep.Render(filter);  
//filterBson.ToString() == {"Name":"aa","_id":NumberLong(123)}

var update = UserRepAsync.Update.Set(nameof(User.CreateTime), DateTime.Now);
BsonDocument updateBson = userRep.Render(update);
//updateBson.ToString() == {"$set":{"CreateTime":ISODate("2017-08-24T07:11:07.185Z")}}

```

