﻿using System;
using System.Collections.Generic;
using MvvmHelpers;
using Syncfusion.SfChart.XForms;
using TurnipTracker.Model;
using TurnipTracker.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace TurnipTracker.ViewModel
{
    public class TrackingViewModel : ViewModelBase
    {

        public List<Day> Days { get; }

        public ObservableRangeCollection<ChartDataModel> ChartData { get; }

        public Command<Day> DaySelectedCommand { get; }

        public TrackingViewModel()
        {
            if (Xamarin.Forms.DesignMode.IsDesignModeEnabled)
                return;


            ChartData = new ObservableRangeCollection<ChartDataModel>();


            Days = DataService.GetCurrentWeek();
            foreach (var day in Days)
                day.SaveCurrentWeekAction = SaveCurrentWeek;



            SelectedDay = Days[(int)DateTime.Now.DayOfWeek];

            DaySelectedCommand = new Command<Day>(OnDaySelected);
        }

        Day selectedDay;

        public Day SelectedDay
        {
            get => selectedDay;
            set
            {
                if (selectedDay == value)
                    return;

                if (selectedDay != null)
                    selectedDay.IsSelectedDay = false;

                SetProperty(ref selectedDay, value, onChanged: UpdatePredications);
                selectedDay.IsSelectedDay = true;
            }
        }

        int min = 0;
        public int Min
        {
            get => min;
            set => SetProperty(ref min, value);
        }

        int max = 0;
        public int Max
        {
            get => max;
            set => SetProperty(ref max, value);
        }


        void OnDaySelected(Day day) => MainThread.BeginInvokeOnMainThread(() =>
                                     {
                                         SelectedDay = day;
                                     });

        public void SaveCurrentWeek()
        {
            DataService.SaveCurrentWeek(Days);
            UpdatePredications();
        }

        bool isGraphExpanded;
        public bool IsGraphExpanded
        {
            get => isGraphExpanded;
            set
            {
                isGraphExpanded = value;
                if (isGraphExpanded)
                    UpdateGraph();
            }
        }

        public void UpdateGraph()
        {
            var chartData = new List<ChartDataModel>();
            var sunday = Days[0];
            foreach (var day in Days)
            {

                if (day == sunday)                {

                    var val = day.BuyPrice ?? 0;
                    chartData.Add(new ChartDataModel("S", day.DayLong, val));
                    continue;
                }

                if (!day.PriceAM.HasValue)
                {
                    chartData.Add(new ChartDataModel($"{day.DayShort} A", $"{day.DayLong} AM", day.PredictionAMMax, day.PredictionAMMin));
                }
                else
                {
                    chartData.Add(new ChartDataModel($"{day.DayShort} A", $"{day.DayLong} AM", day.PriceAM.Value));
                }

                if (!day.PricePM.HasValue)
                {
                    chartData.Add(new ChartDataModel($"{day.DayShort} P", $"{day.DayLong} PM", day.PredictionPMMax, day.PredictionPMMin));
                }
                else
                {
                    chartData.Add(new ChartDataModel($"{day.DayShort} P", $"{day.DayLong} PM", day.PricePM.Value));
                }
            }

            ChartData.ReplaceRange(chartData);
        }

        public void UpdatePredications()
        {
            PredictionUpdater.Update(Days);

            var sunday = Days[0];
            if ((sunday.BuyPrice.HasValue || sunday.ActualPurchasePrice.HasValue) && SelectedDay != sunday)
            {
                var actualBuy = sunday.ActualPurchasePrice.HasValue ? sunday.ActualPurchasePrice.Value : sunday.BuyPrice.Value;
                if (SelectedDay.PriceAM.HasValue)
                {
                    var diffAM = SelectedDay.PriceAM.Value - actualBuy;
                    if (diffAM == 0)
                        SelectedDay.DifferenceAM = "↔️ 0";
                    else
                        SelectedDay.DifferenceAM = diffAM < 0 ? $"↘️ {diffAM}" : $"↗️ +{diffAM}";
                }
                else
                {
                    SelectedDay.DifferenceAM = string.Empty;
                }

                if (SelectedDay.PricePM.HasValue)
                {
                    var diffPM = SelectedDay.PricePM.Value - actualBuy;
                    if (diffPM == 0)
                        SelectedDay.DifferencePM = "↔️ 0";
                    else
                        SelectedDay.DifferencePM = diffPM < 0 ? $"↘️ {diffPM}" : $"↗️ +{diffPM}";
                }
                else
                {
                    SelectedDay.DifferencePM = string.Empty;
                }
            }

            var low = 999;
            var high = 0;
            foreach (var day in Days)
            {

                if (day == sunday)
                {

                    var val = day.BuyPrice ?? 0;
                    continue;
                }
                

                if (!day.PriceAM.HasValue)
                {
                    if (day.PredictionAMMin < low)
                        low = day.PredictionAMMin;

                    if (day.PredictionAMMax > high)
                        high = day.PredictionAMMax;
                }

                if (!day.PricePM.HasValue)
                {
                    if (day.PredictionPMMin < low)
                        low = day.PredictionPMMin;

                    if (day.PredictionPMMax > high)
                        high = day.PredictionPMMax;

                }
            }

            Min = low;
            Max = high;


            if (IsGraphExpanded)
                UpdateGraph();
        }
    }
}
