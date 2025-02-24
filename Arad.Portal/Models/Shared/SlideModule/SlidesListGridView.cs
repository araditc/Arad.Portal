using Arad.Portal.DataLayer.Entities.General.SliderModule;

using System;

namespace Arad.Portal.Models.Shared.SlideModule;

public class SlidesListGridView
{
    public string SliderId { get; set; }
    public string Id { get; init; }
    public string ImageUrl { get; init; }
    public string ColoredBackground { get; init; }
    public string VideoUrl { get; set; }
    public ImageFit ImageFit { get; set; }
    public TransActionType TransActionType { get; set; }
    public string Link { get; init; }
    public Target Target { get; set; }
    public string PersianStartDate { get; init; }
    public string PersianExpireDate { get; init; }

    public DateTime? StartDate { get; init; }

    public DateTime? ExpireDate { get; init; }
    public bool IsActive { get; set; }
    public string Title { get; init; }
    public string Alt { get; set; }
}