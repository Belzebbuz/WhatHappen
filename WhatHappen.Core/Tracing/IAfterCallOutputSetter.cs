namespace WhatHappen.Core.Tracing;

internal interface IAfterCallOutputSetter
{
	void SetOutput(object? output);
}