using Godot;
using System;
using ProjectMina;

public partial class WeaponHUD : Control
{
	protected Control AmmoCounter;
	protected Label CurrentAmmoCounter;
	protected Label ReserveAmmoCounter;

	private WeaponComponent _currentWeapon;
	
	public override void _Ready()
	{
		AmmoCounter = GetNode<Control>("%AmmoCounter");
		CurrentAmmoCounter = GetNode<Label>("%CurrentAmmoCounter");
		ReserveAmmoCounter = GetNode<Label>("%ReserveAmmoCounter");
	}
	
	public void SetWeapon(WeaponComponent weapon)
	{
		if (weapon == null)
		{
			_HideAmmoCounter();
			_DisconnectRangedWeapon((RangedWeaponComponent)_currentWeapon);
			return;
		}
		
		switch (weapon.WeaponType)
		{
			case EWeaponType.Ranged:
				_ConnectRangedWeapon((RangedWeaponComponent)weapon);
				break;
			default:
				break;
		}
	}

	private void _DisconnectWeapon(WeaponComponent weapon)
	{
		switch (weapon.WeaponType)
		{
			case EWeaponType.Ranged:
				_DisconnectRangedWeapon((RangedWeaponComponent)weapon);
				break; 
			default:
				break;
		}
	}

	private void _ConnectRangedWeapon(RangedWeaponComponent weapon)
	{
		_ShowAmmoCounter();
		_currentWeapon = weapon;
		_UpdateAmmoCounter(weapon.CurrentAmmo, 56);
		weapon.AmmoCountUpdated += _UpdateAmmoCounter;
	}

	private void _DisconnectRangedWeapon(RangedWeaponComponent weapon)
	{
		_HideAmmoCounter();
		weapon.AmmoCountUpdated -= _UpdateAmmoCounter;
		_currentWeapon = null;
	}

	private void _UpdateAmmoCounter(int remainingAmmoInClip, int remainingAmmoInReserve)
	{
		CurrentAmmoCounter.Text = remainingAmmoInClip.ToString();
		ReserveAmmoCounter.Text = remainingAmmoInReserve.ToString();
	}

	private void _ShowAmmoCounter()
	{
		AmmoCounter.Visible = true;
	}
	
	private void _HideAmmoCounter()
	{
		AmmoCounter.Visible = false;
	}
}
