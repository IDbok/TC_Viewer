﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcModels.Models.Interfaces;
namespace TcModels.Models.TcContent
{
    public class Outlay: IIdentifiable
    {
        public enum OutlayType
        {
            [Description("Затраты механизмов")] // Тип затрат - затраты механизмов
            Mechine,
            [Description("Затраты сотрудников")] // Тип затрат - затраты сотрудника
            Staff,
            [Description("Материалы")] // Тип затрат - затраты на метариалы/компоненты
            Components,
            [Description("Общее время выполнения работы")] // Тим затрат - общие затраты
            SummaryTimeOutlay,
        }
        public enum UnitType
        {
            [Description("Руб. без НДС")] // Тип затрат - затраты механизмов
            Сurrency,
            [Description("Шт.")] // Тип затрат - затраты сотрудника
            Pieces,
            [Description("Ч.")] // Тип затрат - затраты на метариалы/компоненты
            Hours,
        }
        public int Id { get; set; }
        public int TcId { get; set; }
        public int? ChildId { get; set; }
        public OutlayType Type { get; set; }
        public UnitType OutlayUnitType { get; set; }
        public string? Name { get; set; }
        public double OutlayValue { get; set; }
    }
}