import { CommonModule } from '@angular/common'
import { Component } from '@angular/core'
import { RouterLink } from '@angular/router'

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './home.html',
  styleUrl: './home.css'
})
export class Home {
  categories = [
    {
      icon: 'fa-solid fa-truck-ramp-box',
      name: 'الأوناش',
      description: 'خدمات الرفع والنقل للمواقع والمشروعات'
    },
    {
      icon: 'fa-solid fa-tractor',
      name: 'الحفارات',
      description: 'معدات الحفر والردم وأعمال الأساسات'
    },
    {
      icon: 'fa-solid fa-truck-field',
      name: 'اللوادر',
      description: 'تحميل ونقل الخامات داخل مواقع العمل'
    },
    {
      icon: 'fa-solid fa-bolt-lightning',
      name: 'المولدات',
      description: 'حلول الطاقة المؤقتة للمواقع الصناعية'
    },
    {
      icon: 'fa-solid fa-cubes',
      name: 'الشدات والسقالات',
      description: 'تجهيزات البناء والتشطيبات والمواقع'
    },
    {
      icon: 'fa-solid fa-truck-flatbed',
      name: 'الشاحنات',
      description: 'نقل المعدات والمواد الثقيلة بأمان'
    }
  ]

  stats = [
    {
      value: '+15000',
      label: 'حجز مكتمل'
    },
    {
      value: '+850',
      label: 'مزود خدمة'
    },
    {
      value: '+3200',
      label: 'معدة متاحة'
    },
    {
      value: '99%',
      label: 'نسبة نجاح'
    }
  ]

  features = [
    {
      icon: 'fa-solid fa-magnifying-glass',
      title: 'بحث سريع',
      description: 'اعثر على الخدمة أو المعدة المناسبة حسب الموقع ونوع العمل.'
    },
    {
      icon: 'fa-solid fa-user-shield',
      title: 'مزودون موثقون',
      description: 'مراجعة مستندات وحسابات المزودين قبل ظهورهم للمستخدمين.'
    },
    {
      icon: 'fa-solid fa-wallet',
      title: 'دفع آمن',
      description: 'نظام دفع وتتبع للحجوزات والطلبات مع Escrow داخل المنصة.'
    },
    {
      icon: 'fa-solid fa-comments',
      title: 'تواصل مباشر',
      description: 'رسائل وإشعارات فورية بين العميل والمزود وإدارة الدعم.'
    }
  ]
}