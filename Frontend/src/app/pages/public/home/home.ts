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
      icon: '🏗️',
      name: 'الأوناش',
      description: 'خدمات الرفع والنقل للمواقع والمشروعات'
    },
    {
      icon: '🚜',
      name: 'الحفارات',
      description: 'معدات الحفر والردم وأعمال الأساسات'
    },
    {
      icon: '🚛',
      name: 'اللوادر',
      description: 'تحميل ونقل الخامات داخل مواقع العمل'
    },
    {
      icon: '⚡',
      name: 'المولدات',
      description: 'حلول الطاقة المؤقتة للمواقع الصناعية'
    },
    {
      icon: '🧱',
      name: 'الشدات والسقالات',
      description: 'تجهيزات البناء والتشطيبات والمواقع'
    },
    {
      icon: '🚚',
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
      icon: '🔎',
      title: 'بحث سريع',
      description: 'اعثر على الخدمة أو المعدة المناسبة حسب الموقع ونوع العمل.'
    },
    {
      icon: '🛡️',
      title: 'مزودون موثقون',
      description: 'مراجعة مستندات وحسابات المزودين قبل ظهورهم للمستخدمين.'
    },
    {
      icon: '💳',
      title: 'دفع آمن',
      description: 'نظام دفع وتتبع للحجوزات والطلبات مع Escrow داخل المنصة.'
    },
    {
      icon: '💬',
      title: 'تواصل مباشر',
      description: 'رسائل وإشعارات فورية بين العميل والمزود وإدارة الدعم.'
    }
  ]
}