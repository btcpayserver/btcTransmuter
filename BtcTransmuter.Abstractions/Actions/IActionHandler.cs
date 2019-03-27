using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Abstractions.Actions
{
    public interface IActionHandler
    {
        Type ActionResultDataType { get; }

        Task<ActionHandlerResult> Execute(Dictionary<string, (object data, string json)> data,
            RecipeAction recipeAction);

        Task<bool> CanExecute(Dictionary<string, (object data, string json)> data, RecipeAction recipeAction);
    }
}