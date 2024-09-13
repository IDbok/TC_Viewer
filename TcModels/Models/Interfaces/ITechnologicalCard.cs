using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TcModels.Models.TechnologicalCard;

namespace TcModels.Models.Interfaces
{
    public interface ITechnologicalCard
    {
        public int Id { get; set; }
        public string Article { get; set; }
        public string Name {  get; set; }
        public string? Description {  get; set; }
        public string Version {  get; set; }
        public string Type {  get; set; }
        public float NetworkVoltage { get; set; }
        public string? TechnologicalProcessType { get; set; }
        public string? TechnologicalProcessName {  get; set; }
        public string? TechnologicalProcessNumber {  get; set; }
        public string? Parameter {  get; set; }
        public string? FinalProduct {  get; set; }

        public string? Applicability {  get; set; }
        public string? Note {  get; set; }
        public string? DamageType {  get; set; }
        public string? RepairType {  get; set; }
        public bool IsCompleted {  get; set; }
        public TechnologicalCardStatus Status {  get; set; }

    }
}
