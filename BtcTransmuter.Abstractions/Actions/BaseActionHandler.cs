using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BtcTransmuter.Data;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;

namespace BtcTransmuter.Abstractions.Actions
{
    public abstract class BaseActionHandler<TActionData> : IActionHandler
    {
        public Task<bool> Execute(object triggerData, RecipeAction recipeAction)
        {
            return Execute(triggerData, recipeAction, recipeAction.Get<TActionData>());
        }

        protected abstract Task<bool> Execute(object triggerData, RecipeAction recipeAction, TActionData actionData);

        /// <summary>
        /// https://dotnetfiddle.net/MoqJFk
        /// </summary>
        protected static string InterpolateString(string value, object @object)
        {
            return Regex.Replace(value, @"{(.+?)}",
                match =>
                {
                    var p = Expression.Parameter(@object.GetType(), "TriggerData");
                    var e = System.Linq.Dynamic.DynamicExpression.ParseLambda(new[] {p}, null, match.Groups[1].Value);
                    return (e.Compile().DynamicInvoke(@object) ?? "").ToString();
                });
        }
    }
}