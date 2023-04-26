using System;

namespace Game.Model
{
    public static class EventModelFactory
    {
        public static JSONSerialized EventModelSerialized = new JSONSerialized(typeof(IEventModel),
                                                                               EventModelToJSON,
                                                                               EventModelParseJSON);
        public static JSONObject EventModelToJSON(object obj)
        {
            IEventModel eventModel = obj as IEventModel;
            if (eventModel is null)
                return new JSONObject(JSONObject.Type.NULL);
            Type type = eventModel.GetType();
            JSONObject @object = JSONMap.ToJSON(type, eventModel);
            @object.SetField("class", type.FullName);
            return @object;
        }
        public static object EventModelParseJSON(JSONObject @object)
        {
            if (@object is null || @object.IsNull)
                return null;
            string typeFullName = @object["class"].str;
            Type type = Type.GetType(typeFullName);
            return JSONMap.ParseJSON(type, @object);
        }
    }
}