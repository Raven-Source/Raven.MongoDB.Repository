using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Repository.Funcs
{
    /// <summary>
    /// 
    /// </summary>
    internal static class FunctionScript
    {
//        public static Dictionary<string, string> Item = new Dictionary<string, string>()
//        {
//            ["getNextSequence"] =
//@"function (name, inc) {
//    inc = inc || 1;

//    var mongoSequence = $[MongoSequence]$;
//    var r = db.getCollection(mongoSequence.SequenceName).findAndModify({
//        query:{mongoSequence.CollectionName:name},
//        update:{$inc:{mongoSequence.IncrementID:NumberLong(1)}},
//        upsert: true,
//        new:true
//    });    
//    return r[mongoSequence.IncrementID].toNumber();
//}"
//        };

        public static string getNextSequence =
@"function (name, inc) {
    inc = inc || 1;
   
    var r = db.getCollection('$[SequenceName]$').findAndModify({
        query:{$[CollectionName]$:name},
        update:{$inc:{$[IncrementID]$:NumberLong(1)}},
        upsert: true,
        new:true
    });    
    return r['$[IncrementID]$'].toNumber();
}";

    }
}
