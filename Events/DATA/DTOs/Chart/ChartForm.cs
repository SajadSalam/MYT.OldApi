using System.ComponentModel.DataAnnotations;
using Events.DATA.DTOs.Category;
using Events.Entities;

namespace Events.DATA.DTOs.Chart;

public class ChartForm 
{
    public string Name { get; set; }

    [MinLength(1)]
    public List<CategoryChartForm> Categories { get; set; }
}