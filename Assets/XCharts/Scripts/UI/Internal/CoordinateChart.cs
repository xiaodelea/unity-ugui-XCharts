﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;
using System;

namespace XCharts
{
    public class CoordinateChart : BaseChart
    {
        private static readonly string s_DefaultSplitNameY = "split_y";
        private static readonly string s_DefaultSplitNameX = "split_x";

        [SerializeField] protected Coordinate m_Coordinate = Coordinate.defaultCoordinate;
        [SerializeField] protected XAxis m_XAxis = XAxis.defaultXAxis;
        [SerializeField] protected YAxis m_YAxis = YAxis.defaultYAxis;

        [NonSerialized] private float m_LastXMaxValue;
        [NonSerialized] private float m_LastYMaxValue;
        [NonSerialized] private XAxis m_CheckXAxis = XAxis.defaultXAxis;
        [NonSerialized] private YAxis m_CheckYAxis = YAxis.defaultYAxis;
        [NonSerialized] private Coordinate m_CheckCoordinate = Coordinate.defaultCoordinate;

        protected List<Text> m_SplitYTextList = new List<Text>();
        protected List<Text> m_SplitXTextList = new List<Text>();

        public float zeroX { get { return m_Coordinate.left; } }
        public float zeroY { get { return m_Coordinate.bottom; } }
        public float coordinateWid { get { return chartWidth - m_Coordinate.left - m_Coordinate.right; } }
        public float coordinateHig { get { return chartHeight - m_Coordinate.top - m_Coordinate.bottom; } }
        public Axis xAxis { get { return m_XAxis; } }
        public Axis yAxis { get { return m_YAxis; } }

        protected override void Awake()
        {
            base.Awake();
            InitSplitX();
            InitSplitY();
        }

        protected override void Update()
        {
            base.Update();
            CheckYAxis();
            CheckXAxis();
            CheckMaxValue();
            CheckCoordinate();
        }

        protected override void Reset()
        {
            base.Reset();
            m_Coordinate = Coordinate.defaultCoordinate;
            m_XAxis = XAxis.defaultXAxis;
            m_YAxis = YAxis.defaultYAxis;
            InitSplitX();
            InitSplitY();
        }

        protected override void DrawChart(VertexHelper vh)
        {
            base.DrawChart(vh);
            DrawCoordinate(vh);
        }

        protected override void CheckTootipArea(Vector2 local)
        {
            if (local.x < zeroX || local.x > zeroX + coordinateWid ||
                local.y < zeroY || local.y > zeroY + coordinateHig)
            {
                m_Tooltip.dataIndex = 0;
                RefreshTooltip();
            }
            else
            {
                if (m_XAxis.type == Axis.AxisType.Value)
                {
                    float splitWid = m_YAxis.GetDataWidth(coordinateHig);
                    for (int i = 0; i < m_YAxis.GetDataNumber(); i++)
                    {
                        float pY = zeroY + i * splitWid;
                        if (m_YAxis.boundaryGap)
                        {
                            if (local.y > pY && local.y <= pY + splitWid)
                            {
                                m_Tooltip.dataIndex = i + 1;
                                break;
                            }
                        }
                        else
                        {
                            if (local.y > pY - splitWid / 2 && local.y <= pY + splitWid / 2)
                            {
                                m_Tooltip.dataIndex = i + 1;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    float splitWid = m_XAxis.GetDataWidth(coordinateWid);
                    for (int i = 0; i < m_XAxis.GetDataNumber(); i++)
                    {
                        float pX = zeroX + i * splitWid;
                        if (m_XAxis.boundaryGap)
                        {
                            if (local.x > pX && local.x <= pX + splitWid)
                            {
                                m_Tooltip.dataIndex = i + 1;
                                break;
                            }
                        }
                        else
                        {
                            if (local.x > pX - splitWid / 2 && local.x <= pX + splitWid / 2)
                            {
                                m_Tooltip.dataIndex = i + 1;
                                break;
                            }
                        }
                    }
                }
            }
            if (m_Tooltip.dataIndex > 0)
            {
                m_Tooltip.UpdatePos(new Vector2(local.x + 18, local.y - 25));
                RefreshTooltip();
                if (m_Tooltip.lastDataIndex != m_Tooltip.dataIndex)
                {
                    RefreshChart();
                }
                m_Tooltip.lastDataIndex = m_Tooltip.dataIndex;
            }
        }

        protected override void RefreshTooltip()
        {
            base.RefreshTooltip();
            int index = m_Tooltip.dataIndex - 1;
            Axis tempAxis = m_XAxis.type == Axis.AxisType.Value ? (Axis)m_YAxis : (Axis)m_XAxis;
            if (index < 0)
            {
                m_Tooltip.SetActive(false);
                return;
            }
            m_Tooltip.SetActive(true);
            if (m_Series.Count == 1)
            {
                string txt = tempAxis.GetData(index) + ": " + m_Series.GetData(0,index);
                m_Tooltip.UpdateTooltipText(txt);
            }
            else
            {
                StringBuilder sb = new StringBuilder(tempAxis.GetData(index));
                for (int i = 0; i < m_Series.Count; i++)
                {
                    if (m_Series.series[i].show)
                    {
                        string strColor = ColorUtility.ToHtmlStringRGBA(m_ThemeInfo.GetColor(i));
                        string key = m_Series.series[i].name;
                        float value = m_Series.series[i].data[index];
                        sb.Append("\n");
                        sb.AppendFormat("<color=#{0}>● </color>", strColor);
                        sb.AppendFormat("{0}: {1}", key, value);
                    }
                    
                }
                m_Tooltip.UpdateTooltipText(sb.ToString());
            }
            var pos = m_Tooltip.GetPos();
            if (pos.x + m_Tooltip.width > chartWidth)
            {
                pos.x = chartWidth - m_Tooltip.width;
            }
            if (pos.y - m_Tooltip.height < 0)
            {
                pos.y = m_Tooltip.height;
            }
            m_Tooltip.UpdatePos(pos);
        }

        TextGenerationSettings GetTextSetting()
        {
            var setting = new TextGenerationSettings();
            var fontdata = FontData.defaultFontData;

            //setting.generationExtents = rectTransform.rect.size;
            setting.generationExtents = new Vector2(200.0F, 50.0F);
            setting.fontSize = 14;
            setting.textAnchor = TextAnchor.MiddleCenter;
            setting.scaleFactor = 1f;
            setting.color = Color.red;
            setting.font = m_ThemeInfo.font;
            setting.pivot = new Vector2(0.5f, 0.5f);
            setting.richText = false;
            setting.lineSpacing = 0;
            setting.fontStyle = FontStyle.Normal;
            setting.resizeTextForBestFit = false;
            setting.horizontalOverflow = HorizontalWrapMode.Overflow;
            setting.verticalOverflow = VerticalWrapMode.Overflow;

            return setting;

        }

        protected override void OnThemeChanged()
        {
            base.OnThemeChanged();
            InitSplitX();
            InitSplitY();
        }

        public void AddXAxisData(string category)
        {
            m_XAxis.AddData(category,m_MaxCacheDataNumber);
            OnXAxisChanged();
        }

        public void AddYAxisData(string category)
        {
            m_YAxis.AddData(category, m_MaxCacheDataNumber);
            OnYAxisChanged();
        }

        private void InitSplitY()
        {
            m_SplitYTextList.Clear();
            float max = GetMaxValue();
            float splitWidth = m_YAxis.GetScaleWidth(coordinateHig);

            var titleObject = ChartHelper.AddObject(s_DefaultSplitNameY, transform, chartAnchorMin,
                chartAnchorMax, chartPivot, new Vector2(chartWidth, chartHeight));
            titleObject.transform.localPosition = Vector3.zero;
            ChartHelper.HideAllObject(titleObject, s_DefaultSplitNameY);

            for (int i = 0; i < m_YAxis.splitNumber; i++)
            {
                Text txt = ChartHelper.AddTextObject(s_DefaultSplitNameY + i, titleObject.transform,
                    m_ThemeInfo.font, m_ThemeInfo.textColor, TextAnchor.MiddleRight, Vector2.zero,
                    Vector2.zero, new Vector2(1, 0.5f), new Vector2(m_Coordinate.left, 20),
                    m_Coordinate.fontSize, m_XAxis.textRotation);
                txt.transform.localPosition = GetSplitYPosition(splitWidth, i);
                txt.text = m_YAxis.GetScaleName(i, max);
                txt.gameObject.SetActive(m_YAxis.show);
                m_SplitYTextList.Add(txt);
            }
        }

        public void InitSplitX()
        {
            m_SplitXTextList.Clear();
            float max = GetMaxValue();
            float splitWidth = m_XAxis.GetScaleWidth(coordinateWid);

            var titleObject = ChartHelper.AddObject(s_DefaultSplitNameX, transform, chartAnchorMin,
                chartAnchorMax, chartPivot, new Vector2(chartWidth, chartHeight));
            titleObject.transform.localPosition = Vector3.zero;
            ChartHelper.HideAllObject(titleObject, s_DefaultSplitNameX);

            for (int i = 0; i < m_XAxis.splitNumber; i++)
            {
                Text txt = ChartHelper.AddTextObject(s_DefaultSplitNameX + i, titleObject.transform,
                    m_ThemeInfo.font, m_ThemeInfo.textColor, TextAnchor.MiddleCenter, Vector2.zero,
                    Vector2.zero, new Vector2(1, 0.5f), new Vector2(splitWidth, 20), 
                    m_Coordinate.fontSize, m_XAxis.textRotation);

                txt.transform.localPosition = GetSplitXPosition(splitWidth, i);
                txt.text = m_XAxis.GetScaleName(i, max);
                txt.gameObject.SetActive(m_XAxis.show);
                m_SplitXTextList.Add(txt);
            }
        }

        private Vector3 GetSplitYPosition(float scaleWid, int i)
        {
            if (m_YAxis.boundaryGap)
            {
                return new Vector3(zeroX - m_YAxis.axisTick.length - 2f,
                    zeroY + (i + 0.5f) * scaleWid, 0);
            }
            else
            {
                return new Vector3(zeroX - m_YAxis.axisTick.length - 2f,
                    zeroY + i * scaleWid, 0);
            }
        }

        private Vector3 GetSplitXPosition(float scaleWid, int i)
        {
            if (m_XAxis.boundaryGap)
            {
                return new Vector3(zeroX + (i + 1) * scaleWid, zeroY - m_XAxis.axisTick.length - 5, 0);
            }
            else
            {
                return new Vector3(zeroX + (i + 1 - 0.5f) * scaleWid,
                    zeroY - m_XAxis.axisTick.length - 10, 0);
            }
        }

        private void CheckCoordinate()
        {
            if (m_CheckCoordinate != m_Coordinate)
            {
                m_CheckCoordinate.Copy(m_Coordinate);
                OnCoordinateChanged();
            }
        }

        private void CheckYAxis()
        {
            if (m_CheckYAxis != m_YAxis)
            {
                m_CheckYAxis.Copy(m_YAxis);
                OnYAxisChanged();
            }
        }

        private void CheckXAxis()
        {
            if (!m_CheckXAxis.Equals(m_XAxis))
            {
                m_CheckXAxis.Copy(m_XAxis);
                OnXAxisChanged();
            }
        }

        private void CheckMaxValue()
        {
            if (m_XAxis.type == Axis.AxisType.Value)
            {
                float max = GetMaxValue();
                if (m_LastXMaxValue != max)
                {
                    m_LastXMaxValue = max;
                    OnXMaxValueChanged();
                }
            }
            else if (m_YAxis.type == Axis.AxisType.Value)
            {

                float max = GetMaxValue();
                
                if (m_LastYMaxValue != max)
                {
                    m_LastYMaxValue = max;
                    OnYMaxValueChanged();
                }
            }
        }

        protected virtual void OnCoordinateChanged()
        {
            InitSplitX();
            InitSplitY();
        }

        protected virtual void OnYAxisChanged()
        {
            InitSplitY();
        }

        protected virtual void OnXAxisChanged()
        {
            InitSplitX();
        }

        protected virtual void OnXMaxValueChanged()
        {
            float max = GetMaxValue();
            for (int i = 0; i < m_SplitXTextList.Count; i++)
            {
                m_SplitXTextList[i].text = m_XAxis.GetScaleName(i, max);
            }
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            InitSplitX();
            InitSplitY();
        }

        protected override void OnYMaxValueChanged()
        {
            float max = GetMaxValue();
            for (int i = 0; i < m_SplitYTextList.Count; i++)
            {
                m_SplitYTextList[i].text = m_YAxis.GetScaleName(i, max);
            }
        }

        private void DrawCoordinate(VertexHelper vh)
        {
            #region draw tick and splitline
            if (m_YAxis.show)
            {
                for (int i = 1; i < m_YAxis.GetScaleNumber(); i++)
                {
                    float pX = zeroX - m_YAxis.axisTick.length;
                    float pY = zeroY + i * m_YAxis.GetScaleWidth(coordinateHig);
                    if (m_YAxis.boundaryGap && m_YAxis.axisTick.alignWithLabel)
                    {
                        pY -= m_YAxis.GetScaleWidth(coordinateHig) / 2;
                    }
                    if (m_YAxis.axisTick.show)
                    {
                        ChartHelper.DrawLine(vh, new Vector3(pX, pY), new Vector3(zeroX, pY),
                            m_Coordinate.tickness, m_ThemeInfo.axisLineColor);
                    }
                    if (m_YAxis.showSplitLine)
                    {
                        DrawSplitLine(vh, true, m_YAxis.splitLineType, new Vector3(zeroX, pY),
                            new Vector3(zeroX + coordinateWid, pY));
                    }
                }
            }
            if (m_XAxis.show)
            {
                for (int i = 1; i < m_XAxis.GetScaleNumber(); i++)
                {
                    float pX = zeroX + i * m_XAxis.GetScaleWidth(coordinateWid);
                    float pY = zeroY - m_XAxis.axisTick.length - 2;
                    if (m_XAxis.boundaryGap && m_XAxis.axisTick.alignWithLabel)
                    {
                        pX -= m_XAxis.GetScaleWidth(coordinateWid) / 2;
                    }
                    if (m_XAxis.axisTick.show)
                    {
                        ChartHelper.DrawLine(vh, new Vector3(pX, zeroY), new Vector3(pX, pY), m_Coordinate.tickness,
                        m_ThemeInfo.axisLineColor);
                    }
                    if (m_XAxis.showSplitLine)
                    {
                        DrawSplitLine(vh, false, m_XAxis.splitLineType, new Vector3(pX, zeroY),
                            new Vector3(pX, zeroY + coordinateHig));
                    }
                }
            }
            #endregion

            //draw x,y axis
            if (m_YAxis.show)
            {
                ChartHelper.DrawLine(vh, new Vector3(zeroX, zeroY - m_YAxis.axisTick.length),
                new Vector3(zeroX, zeroY + coordinateHig + 2), m_Coordinate.tickness,
                m_ThemeInfo.axisLineColor);
            }
            if (m_XAxis.show)
            {
                ChartHelper.DrawLine(vh, new Vector3(zeroX - m_XAxis.axisTick.length, zeroY),
                new Vector3(zeroX + coordinateWid + 2, zeroY), m_Coordinate.tickness,
                m_ThemeInfo.axisLineColor);
            }
        }

        private void DrawSplitLine(VertexHelper vh, bool isYAxis, Axis.SplitLineType type, Vector3 startPos,
            Vector3 endPos)
        {
            switch (type)
            {
                case Axis.SplitLineType.Dashed:
                case Axis.SplitLineType.Dotted:
                    var startX = startPos.x;
                    var startY = startPos.y;
                    var dashLen = type == Axis.SplitLineType.Dashed ? 6 : 2.5f;
                    var count = isYAxis ? (endPos.x - startPos.x) / (dashLen * 2) :
                        (endPos.y - startPos.y) / (dashLen * 2);
                    for (int i = 0; i < count; i++)
                    {
                        if (isYAxis)
                        {
                            var toX = startX + dashLen;
                            ChartHelper.DrawLine(vh, new Vector3(startX, startY), new Vector3(toX, startY),
                                m_Coordinate.tickness, m_ThemeInfo.axisSplitLineColor);
                            startX += dashLen * 2;
                        }
                        else
                        {
                            var toY = startY + dashLen;
                            ChartHelper.DrawLine(vh, new Vector3(startX, startY), new Vector3(startX, toY),
                                m_Coordinate.tickness, m_ThemeInfo.axisSplitLineColor);
                            startY += dashLen * 2;
                        }

                    }
                    break;
                case Axis.SplitLineType.Solid:
                    ChartHelper.DrawLine(vh, startPos, endPos, m_Coordinate.tickness,
                        m_ThemeInfo.axisSplitLineColor);
                    break;
            }
        }
    }
}
