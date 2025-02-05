using Microsoft.AspNetCore.Mvc;
using SmartParking.CalculationsEndpoint;
using SmartParking.CustomerPackages;
using SmartParking.Entities.Organizations;
using SmartParking.GarageTransactions;
using System;
using SmartParking.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace SmartParking.CalculationsEndPoints;
public class CalculationsEndPointsAppService : ITransientDependency, ICalculationsEndPointsAppService 
{

    public int CalculateTime(DateTime startTime, DateTime endTime, int FreeMinutes)
    {


        // Calculate the time difference
        TimeSpan timeDifference = endTime - startTime;



        if ((int)timeDifference.TotalHours == 0 && timeDifference.Minutes <= FreeMinutes)
        {
            return 0;

        }

        int hoursDiff = (int)timeDifference.TotalHours;


        hoursDiff += (timeDifference.Minutes > 0 ? 1 : 0);


        return  hoursDiff;
    }

    public  decimal CalculatePriceForExceededHours(int totalHours, Organization organization)
    {
        decimal price = 0;
        if (totalHours > organization.NumberOfHours)
        {
            int fullDaysCustomerHasStayed = 0;
            if (totalHours >= 24)
            {
                fullDaysCustomerHasStayed = totalHours / 24;
                totalHours %= 24;
            }
            else
            {
                fullDaysCustomerHasStayed = 1;
                totalHours = 0;
            }
            price += (fullDaysCustomerHasStayed * organization.FullDayPrice);
        }
        price += CalculatePriceHours(organization, totalHours);
        return price;

    }
    public  decimal CalculatePriceForOrganizationIsClosedAndExceededHours(int totalHours, Organization organization, CreateUpdateGarageTransactionDto garageTransaction, DateTime entrenceTime)
    {
        (double specialHours, double normalHours) = CalculateHours(garageTransaction, entrenceTime, organization);
        var closingHour = Math.Ceiling((organization.ClosingHoursEnd - organization.ClosingHoursStart).TotalHours);
        double price = 0;
        double totalDays = Math.Ceiling(specialHours / closingHour);
        price += totalDays * (double)organization.FullDayPrice;
        price += ((int)(normalHours / 24)) * (double)organization.FullDayPrice;
        normalHours -= ((int)(normalHours / 24)) * 24;
        if (normalHours > organization.NumberOfHours)
        {
            price += (double)organization.FullDayPrice;
            normalHours = 0;
        }
        price += (double) CalculatePriceHours(organization, (int)normalHours);
        return (decimal)price;

    }
    public  decimal CalculatePriceForOrganizationIsClosed(int totalHours, Organization organization, CreateUpdateGarageTransactionDto garageTransaction, DateTime entrenceTime)
    {

        //special hours -> hours where organization is close
        //normal hours -> hours where organization is open 
        (double specialHours, double normalHours) = CalculateHours(garageTransaction, entrenceTime, organization);
        var closingHour = Math.Ceiling((organization.ClosingHoursEnd - organization.ClosingHoursStart).TotalHours);
        double price = 0;
        double totalDays = Math.Ceiling(specialHours / closingHour==0?1: closingHour);
        // calculates full days price
        price += totalDays * (double)organization.FullDayPrice;
        //calculates hours
        if ((int)normalHours + (int)specialHours > 24)
        {
            price += (double)  CalculatePriceHoursExceededDay(organization, (int)normalHours + (int)specialHours);
            price -= totalDays * closingHour * (double)organization.AdditionalHourPrice;

        }
        else
        {
            price += (double)  CalculatePriceHoursExceededDay(organization, (int)normalHours);

        }
        return (decimal)price;
    }
    public  decimal CalculatePriceHours(Organization organization, int hours)
    {
        decimal price = 0;
        price += hours > 0 ? organization.FirstHourPrice : 0;
        hours--;
        price += hours > 0 ? organization.SecondHourPrice : 0;
        hours--;
        price += hours > 0 ? organization.ThirdHourPrice : 0;
        hours--;
        price += hours > 0 ? (hours * organization.AdditionalHourPrice) : 0;
        return price;
    }
    public decimal CalculatePriceHoursExceededDay(Organization organization, int hours)
    {
        decimal price = 0;
        while (hours > 0)
        {
            price += hours > 0 ? organization.FirstHourPrice : 0;
            hours--;
            price += hours > 0 ? organization.SecondHourPrice : 0;
            hours--;
            price += hours > 0 ? organization.ThirdHourPrice : 0;
            hours--;
            price += hours > 0 ? (Math.Min(hours, 21) * organization.AdditionalHourPrice) : 0;
            hours -= 21;

        }
        return price;
    }
    public (double, double) CalculateHours(CreateUpdateGarageTransactionDto garageTransaction, DateTime entrenceTime, Organization organization)
    {
        TimeSpan totalDuration = garageTransaction.AccurateTime.Value - entrenceTime;

        double totalHours = (int)totalDuration.TotalHours;
        totalHours += totalDuration.Minutes > 0 ? 1 : 0;


        double specialHours = 0;

        // Loop through each day from entry date to exit date
        DateTime currentDate = entrenceTime.Date;
        while (currentDate <= garageTransaction.AccurateTime.Value.Date)
        {
            DateTime specialStartTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, organization.ClosingHoursStart.Hour, organization.ClosingHoursStart.Minute, 0);
            DateTime specialEndTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, organization.ClosingHoursEnd.Hour, organization.ClosingHoursEnd.Minute, 0);

            // Handle overnight case where special hours span two dates
            if (specialEndTime.Hour < specialStartTime.Hour)
            {
                specialEndTime = specialEndTime.AddDays(1);

                // Calculate hours before midnight
                if (entrenceTime < specialEndTime && garageTransaction.AccurateTime >= specialStartTime)
                {
                    DateTime effectiveStart = entrenceTime < specialStartTime ? specialStartTime : entrenceTime;
                    DateTime effectiveEnd = garageTransaction.AccurateTime.Value < specialEndTime ? garageTransaction.AccurateTime.Value : specialEndTime;

                    TimeSpan specialDurationBeforeMidnight = effectiveEnd - effectiveStart;
                    if (specialDurationBeforeMidnight.TotalHours > 0)
                    {
                        specialHours += (int)specialDurationBeforeMidnight.TotalHours; // Round up
                        specialHours += specialDurationBeforeMidnight.Minutes > 0 ? 1 : 0;
                    }
                }

                // Calculate hours after midnight
                if (garageTransaction.AccurateTime > specialEndTime && garageTransaction.AccurateTime >= specialEndTime)
                {
                    DateTime effectiveStart = specialEndTime;
                    DateTime effectiveEnd = garageTransaction.AccurateTime.Value < specialEndTime ? garageTransaction.AccurateTime.Value : specialEndTime;

                    TimeSpan specialDurationAfterMidnight = effectiveEnd - effectiveStart;
                    if (specialDurationAfterMidnight.TotalHours > 0)
                    {
                        specialHours += (int)specialDurationAfterMidnight.TotalHours; // Round up
                        specialHours += specialDurationAfterMidnight.Minutes > 0 ? 1 : 0;
                    }
                }

            }
            else
            {
                // Normal case where special hours are within the same day
                if (entrenceTime < specialEndTime && garageTransaction.AccurateTime > specialStartTime)
                {
                    DateTime effectiveStart = entrenceTime < specialStartTime ? specialStartTime : entrenceTime;
                    DateTime effectiveEnd = garageTransaction.AccurateTime.Value < specialEndTime ? garageTransaction.AccurateTime.Value : specialEndTime;

                    TimeSpan specialDuration = effectiveEnd - effectiveStart;
                    if (specialDuration.TotalHours > 0)
                    {
                        specialHours += (int)specialDuration.TotalHours; // Round up
                        specialHours += specialDuration.Minutes > 0 ? 1 : 0;
                    }
                }
            }

            // Move to the next day
            currentDate = currentDate.AddDays(1);
        }

        // Normal hours are total hours minus special hours
        double normalHours = totalHours - specialHours;
        return (specialHours, normalHours);
    }

    public int CalculateTimeDependsOnPackage(DateTime? entranceTime, DateTime? exitTime, CustomerPackageDto packageDto, DateTime? PackageExpiryDate)
    {
        if (exitTime < entranceTime)
        {
            throw new BusinessException("Leaving date can't be befor entrance date");
        }
        if (packageDto.publishedOfPackage.Value)
        {
            return 0;
        }
        DateTime packageStart = entranceTime.Value.Date.AddHours(packageDto.PublishFromOfPackage.Value.Hour).AddMinutes(packageDto.Package.PublishFrom.Minute);
        DateTime packageEnd = entranceTime.Value.Date.AddHours(packageDto.PublishToOfPackage.Value.Hour).AddMinutes(packageDto.Package.PublishTo.Minute);

        DateInt EntryDateInt, ExitDateInt, PackageStartDateInt, PackageEndDateInt, minimumDateInt;
        ChangeToMyCustomDataType(entranceTime.Value, exitTime.Value, packageStart, packageEnd, PackageExpiryDate.Value, out EntryDateInt, out ExitDateInt, out PackageStartDateInt, out PackageEndDateInt, out minimumDateInt);

        int TotalHours = (int)Math.Ceiling((exitTime.Value - entranceTime.Value).TotalHours);
        int TotalTimeInsidePackage = 0;

        var daysInsidePackage = (minimumDateInt.DayOfYear - EntryDateInt.DayOfYear);
        var totalHoursOfPackage = PackageDuration(packageStart, packageEnd);
        TotalTimeInsidePackage += (daysInsidePackage * totalHoursOfPackage) + totalHoursOfPackage;
        if (packageStart.TimeOfDay > packageEnd.TimeOfDay)
        {
            EntryDateInt.Hours += 24;
            ExitDateInt.Hours += 24;
            PackageStartDateInt.Hours += 24;
            PackageEndDateInt.Hours += 24;
        }
        //cases of entrance
        if (EntryDateInt.Hours > PackageStartDateInt.Hours)
        {
            TotalTimeInsidePackage = HandleEntranceCases(EntryDateInt, PackageEndDateInt, TotalTimeInsidePackage, totalHoursOfPackage);

        }
        //cases of left
        // remove Time of day and replace with my custom object.
        if ((minimumDateInt.Hours < PackageEndDateInt.Hours) && ExitDateInt.Date == EntryDateInt.Date)
        {
            TotalTimeInsidePackage = CasesOfLeft(minimumDateInt, PackageStartDateInt, TotalTimeInsidePackage, totalHoursOfPackage);
        }
        if (ExitDateInt.Date == EntryDateInt.Date)
        {
            return TotalHours - TotalTimeInsidePackage;
        }
        //cases of left
        //cases of left when package has been ended
        if ((minimumDateInt.Hours < PackageEndDateInt.Hours))
        {
            TotalTimeInsidePackage = CasesOfLeft(ExitDateInt, PackageStartDateInt, TotalTimeInsidePackage, totalHoursOfPackage);
        }
        var result = TotalHours - TotalTimeInsidePackage;
        return Math.Max(result, 0);

    }

    private void ChangeToMyCustomDataType(DateTime EntryDate, DateTime ExitDate, DateTime PackageStartTime, DateTime PackageEndTime, DateTime ExpiryDate, out DateInt EntryDateInt, out DateInt ExitDateInt, out DateInt PackageStartDateInt, out DateInt PackageEndDateInt, out DateInt minimumDateInt)
    {
        EntryDateInt = DateTimeToDateInt(EntryDate);
        ExitDateInt = DateTimeToDateInt(ExitDate);
        PackageStartDateInt = DateTimeToDateInt(PackageStartTime);
        PackageEndDateInt = DateTimeToDateInt(PackageEndTime);
        DateTime minimumDate = MinDate(ExitDate, ExpiryDate);
        minimumDateInt = DateTimeToDateInt(minimumDate);
    }

    private static int HandleEntranceCases(DateInt EntryDate, DateInt PackageEndTime, int TotalTimeInsidePackage, int totalHoursOfPackage)
    {
        if (EntryDate.TimeOfDay < PackageEndTime.TimeOfDay)
        {
            var timeOustidePackage = totalHoursOfPackage - CalculatesHoursBetweenTwoTimes(PackageEndTime, EntryDate); ;
            TotalTimeInsidePackage -= timeOustidePackage;
        }
        else
        {
            TotalTimeInsidePackage -= totalHoursOfPackage;

        }

        return TotalTimeInsidePackage;
    }
    private static int PackageDuration(DateTime PackageStart, DateTime PackageEnd)
    {
        var hours = (PackageEnd.TimeOfDay - PackageStart.TimeOfDay).Hours;
        if (hours < 0)
        {
            hours += 24;
        }
        var minutes = (PackageEnd.TimeOfDay - PackageEnd.TimeOfDay).Minutes;
        if (minutes < 0)
        {
            minutes += 60;
        }
        return hours + (minutes > 0 ? 1 : 0);


    }
    private DateInt DateTimeToDateInt(DateTime Date)
    {
        return new DateInt
        {
            DayOfYear = Date.DayOfYear,
            Hours = Date.Hour,
            Minutes = Date.Minute,
            TimeOfDay = Date.TimeOfDay,
            Date = Date

        };
    }
    private static int CalculatesHoursBetweenTwoTimes(DateInt PackageStartTime, DateInt PackageEndTime)
    {
        return ((Math.Abs((PackageEndTime.TimeOfDay - PackageStartTime.TimeOfDay).Hours) + ((Math.Abs((PackageEndTime.TimeOfDay - PackageStartTime.TimeOfDay).Minutes)) > 0 ? 1 : 0)));
    }

    private static int CasesOfLeft(DateInt ExitDate, DateInt PackageStartTime, int TotalTimeInsidePackage, int totalHoursOfPackage)
    {
        if (ExitDate.TimeOfDay < PackageStartTime.TimeOfDay)
        {
            TotalTimeInsidePackage -= totalHoursOfPackage;
        }
        else
        {
            TotalTimeInsidePackage -= (totalHoursOfPackage - (Math.Abs((ExitDate.TimeOfDay.Hours - PackageStartTime.TimeOfDay.Hours)) + ((ExitDate.TimeOfDay.Minutes - PackageStartTime.TimeOfDay.Minutes) > 0 ? 1 : 0)));

        }

        return TotalTimeInsidePackage;
    }

    public static DateTime MinDate(DateTime T1, DateTime T2) => (T1.Date.Date <= T2.Date.Date && T1.Date.TimeOfDay <= T2.Date.TimeOfDay) ? T1 : T2;
    public static DateTime MaxTimeOfDay(DateTime T1, DateTime T2) => T1.Date.TimeOfDay > T2.Date.TimeOfDay ? T1 : T2;
    public static DateTime MinTimeOfDay(DateTime T1, DateTime T2) => T1.Date.TimeOfDay > T2.Date.TimeOfDay ? T2 : T1;



}
