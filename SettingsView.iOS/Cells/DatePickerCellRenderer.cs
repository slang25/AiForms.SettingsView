﻿using System;
using AiForms.Renderers;
using AiForms.Renderers.iOS;
using CoreGraphics;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(DatePickerCell), typeof(DatePickerCellRenderer))]
namespace AiForms.Renderers.iOS
{
    public class DatePickerCellRenderer : CellBaseRenderer<DatePickerCellView> { }

    public class DatePickerCellView : LabelCellView, IPickerCell
    {
        DatePickerCell _DatePickerCell => Cell as DatePickerCell;
        public UITextField DummyField { get; set; }
        NSDate _preSelectedDate;
        UIDatePicker _picker;

        public DatePickerCellView(Cell formsCell) : base(formsCell)
        {
            DummyField = new NoCaretField();
            DummyField.BorderStyle = UITextBorderStyle.None;
            DummyField.BackgroundColor = UIColor.Clear;
            ContentView.AddSubview(DummyField);
            ContentView.SendSubviewToBack(DummyField);

            SelectionStyle = UITableViewCellSelectionStyle.Default;

            SetUpDatePicker();
        }

        public override void UpdateCell()
        {
            base.UpdateCell();
            UpdateMaximumDate();
            UpdateMinimumDate();
            UpdateDate();
        }

        public override void CellPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.CellPropertyChanged(sender, e);
            if (e.PropertyName == DatePickerCell.DateProperty.PropertyName ||
                e.PropertyName == DatePickerCell.FormatProperty.PropertyName) {
                UpdateDate();
            }
            else if (e.PropertyName == DatePickerCell.TodayTextProperty.PropertyName) {
                UpdateTodayText();
            }
            else if (e.PropertyName == DatePickerCell.MaximumDateProperty.PropertyName) {
                UpdateMaximumDate();
            }
            else if (e.PropertyName == DatePickerCell.MinimumDateProperty.PropertyName) {
                UpdateMinimumDate();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                DummyField.RemoveFromSuperview();
                DummyField?.Dispose();
                DummyField = null;
                _picker.Dispose();
                _picker = null;
            }
            base.Dispose(disposing);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            DummyField.Frame = new CGRect(0, 0, Frame.Width, Frame.Height);
        }

        void SetUpDatePicker()
        {
            _picker = new UIDatePicker { Mode = UIDatePickerMode.Date, TimeZone = new Foundation.NSTimeZone("UTC") };

            var width = UIScreen.MainScreen.Bounds.Width;
            var toolbar = new UIToolbar(new CGRect(0, 0, width, 44)) { BarStyle = UIBarStyle.Default, Translucent = true };

            var cancelButton = new UIBarButtonItem(UIBarButtonSystemItem.Cancel, (o, e) =>
            {
                DummyField.ResignFirstResponder();
                _picker.Date = _preSelectedDate;
            });

            var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
            var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, (o, a) =>
            {
                DummyField.ResignFirstResponder();
                Done();
            });

            if (!string.IsNullOrEmpty(_DatePickerCell.TodayText)) {
                var labelButton = new UIBarButtonItem(_DatePickerCell.TodayText, UIBarButtonItemStyle.Plain, (sender, e) =>
                {
                    SetToday();
                });
                var fixspacer = new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace) { Width = 20 };
                toolbar.SetItems(new[] { cancelButton, spacer, labelButton, fixspacer, doneButton }, false);
            }
            else {
                toolbar.SetItems(new[] { cancelButton, spacer, doneButton }, false);
            }

            DummyField.InputView = _picker;
            DummyField.InputAccessoryView = toolbar;
        }

        void SetToday()
        {
            if (_picker.MinimumDate.ToDateTime() <= DateTime.Today && _picker.MaximumDate.ToDateTime() >= DateTime.Today) {
                _picker.SetDate(DateTime.Today.ToNSDate(), true);
            }
        }

        void Done()
        {
            _DatePickerCell.Date = _picker.Date.ToDateTime().Date;
            ValueLabel.Text = _DatePickerCell.Date.ToString(_DatePickerCell.Format);
            _preSelectedDate = _picker.Date;
        }

        void UpdateDate()
        {
            _picker.SetDate(_DatePickerCell.Date.ToNSDate(), true);
            ValueLabel.Text = _DatePickerCell.Date.ToString(_DatePickerCell.Format);
            _preSelectedDate = _DatePickerCell.Date.ToNSDate();
        }

        void UpdateMaximumDate()
        {
            _picker.MaximumDate = _DatePickerCell.MaximumDate.ToNSDate();
        }

        void UpdateMinimumDate()
        {
            _picker.MinimumDate = _DatePickerCell.MinimumDate.ToNSDate();
        }

        void UpdateTodayText()
        {
            SetUpDatePicker();
            UpdateDate();
            UpdateMaximumDate();
            UpdateMinimumDate();
        }
    }
}
