using BusinessLogic.DTO.RiskAnalysis;
using DataManagement.Models;

namespace BusinessLogic.Services;

public interface IRiskCalculatorService
{
    RiskAnalysis CalculateRisk(Wallet wallet);
}