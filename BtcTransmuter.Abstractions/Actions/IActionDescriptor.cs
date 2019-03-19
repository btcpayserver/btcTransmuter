using System;
using System.Threading.Tasks;
using BtcTransmuter.Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.Abstractions.Actions
{
    public interface IActionDescriptor
    {
        string ActionId { get; }
        string Name { get; }
        string Description { get; }

        string ViewPartial { get; }
        Type ActionResultDataType { get; }
        Task<IActionResult> EditData(RecipeAction data);
    }
}