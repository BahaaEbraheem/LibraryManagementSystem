from django import forms
from .models import Book,Category


class bookForm(forms.ModelForm):
    class Meta:
        model = Book
        fields = '__all__'
        # wedgets={
        #     'title':forms.TextInput(attrs={'class':'form-control'}),
        #     'author':forms.TextInput(attrs={'class':'form-control'}),
        #     'photo_book':forms.FileField(attrs={'class':'form-control'}),
        #     'category':forms.Select(attrs={'class':'form-control'}),
        # }
        widgets = {
            'retal_price_day': forms.NumberInput(attrs={
                'class': 'form-control',
                'placeholder': 'سعر الإيجار اليومي',
                'id': 'id_retal_price_day'  # You can override ID if needed
            }),
            'retal_period': forms.NumberInput(attrs={
                'class': 'form-control',
                'placeholder': 'مدة الإيجار (أيام)',
                'id': 'id_retal_period'
            }),
            'total_rental': forms.NumberInput(attrs={
                'class': 'form-control',
                'readonly': True,
                'id': 'id_total_rental'
            }),
        }

class CategoryForm(forms.ModelForm):
      class Meta:
           model = Category
           fields = '__all__'