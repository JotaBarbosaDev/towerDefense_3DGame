using System;
using Core.Economy;
using UnityEngine;

public class SimpleCurrencyManager : MonoBehaviour
{
    public static SimpleCurrencyManager instance { get; private set; }

    [Min(0)] public int startingCurrency = 20;
    [SerializeField] int m_CurrentCurrency;

    public Currency currency { get; private set; }

    public int currentCurrency
    {
        get { return currency == null ? m_CurrentCurrency : currency.currentCurrency; }
    }

    public event Action<int> currencyChanged;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        InitializeCurrency();
    }

    public void InitializeCurrency()
    {
        if (currency != null)
        {
            currency.currencyChanged -= OnCurrencyChanged;
        }

        currency = new Currency(Mathf.Max(0, startingCurrency));
        currency.currencyChanged += OnCurrencyChanged;
        m_CurrentCurrency = currency.currentCurrency;
        NotifyCurrencyChanged();
    }

    public void AddCurrency(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        if (currency == null)
        {
            InitializeCurrency();
        }

        currency.AddCurrency(amount);
    }

    public bool CanAfford(int amount)
    {
        if (currency == null)
        {
            InitializeCurrency();
        }

        return currency.CanAfford(amount);
    }

    public bool TrySpendCurrency(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (currency == null)
        {
            InitializeCurrency();
        }

        return currency.TryPurchase(amount);
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }

        if (currency != null)
        {
            currency.currencyChanged -= OnCurrencyChanged;
        }
    }

    void OnCurrencyChanged()
    {
        m_CurrentCurrency = currency.currentCurrency;
        NotifyCurrencyChanged();
    }

    void NotifyCurrencyChanged()
    {
        if (currencyChanged != null)
        {
            currencyChanged(currentCurrency);
        }
    }
}
