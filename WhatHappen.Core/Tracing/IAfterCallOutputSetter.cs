namespace WhatHappen.Core.Tracing;

public interface IAfterCallOutputSetter
{
	void SetOutput(object? output);
}