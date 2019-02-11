using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BtcTransmuter.Data;
using BtcTransmuter.Extension.Email.Actions;

namespace BtcTransmuter.Extension.Email.Actions
{
    public class SendEmailData
    {
    }


    public interface IActionHandler
    {
        Task<bool> Execute(object triggerData, RecipeAction recipeAction);
    }

    public abstract class BaseActionHandler<T> : IActionHandler
    {
        public Task<bool> Execute(object triggerData, RecipeAction recipeAction)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// https://dotnetfiddle.net/MoqJFk
        /// </summary>
        /// <param name="value"></param>
        /// <param name="object"></param>
        /// <returns></returns>
        protected static string ReplaceMacro(string value, object @object)
        {
            return Regex.Replace(value, @"{(.+?)}",
                match =>
                {
                    var p = Expression.Parameter(@object.GetType(), @object.GetType().Name);
                    //Console.WriteLine("{0} {1}",job.GetType(), job.GetType().Name);
                    var e = System.Linq.Dynamic.DynamicExpression.ParseLambda(new[] {p}, null, match.Groups[1].Value);
                    return (e.Compile().DynamicInvoke(@object) ?? "").ToString();
                });
        }
    }


    public class SendEmailDataActionHandler : BaseActionHandler<SendEmailData>
    {
    }


    public interface IActionDispatcher
    {
        Task Dispatch();
    }
}