﻿using Spectrum.API;
using Spectrum.API.Game;
using Spectrum.API.Game.Vehicle;
using Spectrum.API.Interfaces.Plugins;
using Spectrum.API.Interfaces.Systems;
using Spectrum.API.Configuration;
using System;

namespace Spectrum.Plugins.Heat
{

    public enum Units {kph, mph};
    public enum Display {watermark, hud, car};
    public enum Activation {always, warning, toggle};
    public class Entry : IPlugin, IUpdatable
    {
        public string FriendlyName => "Heat";
        public string Author => "pigpenguin";
        public string Contact => "pigpenguin@gmail.com";
        public APILevel CompatibleAPILevel => APILevel.UltraViolet;

        private bool toggled = false;
        private double warningThreshold;
        private Units units;
        private Display display;
        private Activation activation;
        private Settings _settings;

        public void Initialize(IManager manager)
        {
            _settings = new Settings(typeof(Entry));
            ValidateSettings();
            units = (Units)Enum.Parse(typeof(Units),_settings.GetItem<string>("units"));
            display = (Display)Enum.Parse(typeof(Display),_settings.GetItem<string>("display"));
            activation = (Activation)Enum.Parse(typeof(Activation),_settings.GetItem<string>("activation"));
            warningThreshold = _settings.GetItem<double>("warningThreshold");
            manager.Hotkeys.Bind(_settings.GetItem<string>("toggleHotkey"), () => { toggled = !toggled; Game.WatermarkText = ""; });
        }

        public void Update()
        {
            if (DisplayEnabled())
            {
                if (display == Display.hud)
                    LocalVehicle.HUD.SetHUDText(DisplayText());
                else
                    Game.WatermarkText = DisplayText();
            }

        }
        private string HeatPercent()
        {
            return Convert.ToInt32(LocalVehicle.HeatLevel * 100).ToString() + "% Heat";
        }
        private string Speed()
        {
            if(units == Units.kph)
                return Convert.ToInt32(LocalVehicle.VelocityKPH).ToString() + "KPH";

            return Convert.ToInt32(LocalVehicle.VelocityMPH).ToString() + "MPH";
        }
        private string DisplayText() {
            return HeatPercent() + "\n" + Speed();
        }
        private bool DisplayEnabled()
        {
            try
            {
                return (activation == Activation.always) ||
                       (activation == Activation.warning && LocalVehicle.HeatLevel > warningThreshold) ||
                       (activation == Activation.toggle && toggled);
            }
            catch
            {
                return false;
            }
        }
        
        private void ValidateSettings()
        {
            if (!_settings.ContainsKey("toggleHotkey"))
                _settings.Add("toggleHotkey", "LeftControl+H");
            
            if (!_settings.ContainsKey("units"))
                _settings.Add("units", "kph");

            if (!_settings.ContainsKey("display"))
                _settings.Add("display", "watermark");

            if (!_settings.ContainsKey("activation"))
                _settings.Add("activation", "always");

            if (!_settings.ContainsKey("warningThreshold"))
                _settings.Add("warningThreshold", 0.80);

            _settings.Save();
        }

        public void Shutdown()
        {

        }
    }
}